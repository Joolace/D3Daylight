using SharpGen.Runtime;
using System;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Windows.Forms;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using static Vortice.Direct3D11.D3D11;
using static Vortice.DXGI.DXGI;

public class D3DRenderer : Form
{
    // D3D11 and rendering objects
    private string _imagePath;
    private ID3D11Device device;
    private ID3D11DeviceContext context;
    private IDXGISwapChain1 swapChain;
    private ID3D11RenderTargetView rtv;
    private ID3D11ShaderResourceView textureView;
    private ID3D11SamplerState sampler;
    private ID3D11VertexShader vertexShader;
    private ID3D11PixelShader pixelShader;
    private ID3D11InputLayout inputLayout;
    private ID3D11Buffer vertexBuffer;

    // Constructor — initializes form and stores the image path
    public D3DRenderer(string imagePath)
    {
        _imagePath = imagePath;
        Text = Path.GetFileNameWithoutExtension(imagePath);
        Width = 1920;
        Height = 1080;
        StartPosition = FormStartPosition.CenterScreen;
        Shown += OnShown; // Register event handler to initialize rendering after window is shown
    }

    // Loads a PNG/JPG image into a Direct3D texture and sets up the sampler
    private void LoadImageTexture()
    {
        using var bitmap = new System.Drawing.Bitmap(_imagePath);

        // Loads a PNG/JPG image into a Direct3D texture and sets up the sampler
        bitmap.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipY);

        var data = bitmap.LockBits(
            new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
            System.Drawing.Imaging.ImageLockMode.ReadOnly,
            System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        var textureDesc = new Texture2DDescription
        {
            Width = (uint)bitmap.Width,
            Height = (uint)bitmap.Height,
            MipLevels = 1,
            ArraySize = 1,
            Format = Format.B8G8R8A8_UNorm,
            SampleDescription = new SampleDescription(1, 0),
            Usage = ResourceUsage.Immutable,
            BindFlags = BindFlags.ShaderResource,
        };

        var dataBox = new SubresourceData(data.Scan0, (uint)data.Stride, 0);
        using var texture = device.CreateTexture2D(textureDesc, new[] { dataBox });
        textureView = device.CreateShaderResourceView(texture);
        bitmap.UnlockBits(data);

        // Create sampler to sample the texture
        sampler = device.CreateSamplerState(new SamplerDescription
        {
            Filter = Filter.MinMagMipLinear,
            AddressU = TextureAddressMode.Wrap,
            AddressV = TextureAddressMode.Wrap,
            AddressW = TextureAddressMode.Wrap
        });
    }

    // Called when the window is shown — sets up D3D device, swapchain, backbuffer and resources
    private void OnShown(object sender, EventArgs e)
    {
        var swapDesc = new SwapChainDescription1()
        {
            BufferCount = 2,
            Width = (uint)ClientSize.Width,
            Height = (uint)ClientSize.Height,
            Format = Format.R8G8B8A8_UNorm,
            BufferUsage = Usage.RenderTargetOutput,
            SampleDescription = new SampleDescription(1, 0),
            SwapEffect = SwapEffect.FlipDiscard,
        };

        using var factory = CreateDXGIFactory2<IDXGIFactory2>(false);
        D3D11CreateDevice(null, DriverType.Hardware, DeviceCreationFlags.BgraSupport,
            new[] { FeatureLevel.Level_11_0 }, out device, out context);

        swapChain = factory.CreateSwapChainForHwnd(device, Handle, swapDesc);

        var backBuffer = swapChain.GetBuffer<ID3D11Texture2D>(0);
        rtv = device.CreateRenderTargetView(backBuffer);
        backBuffer.Dispose();

        LoadImageTexture();
        SetupShaders();
        SetupQuad();

        context.RSSetViewport(0, 0, ClientSize.Width, ClientSize.Height); // Set full-screen viewport
        System.Windows.Forms.Application.Idle += RenderLoop; // Start rendering when app is idle
    }

    private ID3D11Buffer resolutionBuffer;

    // Compiles shaders and creates a constant buffer with image/screen resolution to preserve aspect ratio
    private void SetupShaders()
    {
        // Vertex Shader with logic to scale image while preserving aspect ratio
        string vsCode = @"
cbuffer Resolution : register(b0)
{
    float2 imageSize;
    float2 screenSize;
};

struct VSInput {
    float3 position : POSITION;
    float2 texcoord : TEXCOORD;
};

struct PSInput {
    float4 position : SV_POSITION;
    float2 texcoord : TEXCOORD;
};

PSInput VSMain(VSInput input) {
    PSInput output;

    float aspectImage = imageSize.x / imageSize.y;
    float aspectScreen = screenSize.x / screenSize.y;

    float scaleX = 1.0;
    float scaleY = 1.0;

    if (aspectImage > aspectScreen)
        scaleY = aspectScreen / aspectImage;
    else
        scaleX = aspectImage / aspectScreen;

    float3 scaledPos = float3(input.position.x * scaleX, input.position.y * scaleY, input.position.z);

    output.position = float4(scaledPos, 1.0);
    output.texcoord = input.texcoord;
    return output;
}";

        // Pixel Shader — simple texture sampler
        string psCode = @"
Texture2D tex : register(t0);
SamplerState samp : register(s0);

float4 PSMain(float4 position : SV_POSITION, float2 texcoord : TEXCOORD) : SV_TARGET {
    return tex.Sample(samp, texcoord);
}";

        // Save shaders to temp files and compile them
        string vsPath = Path.GetTempFileName();
        string psPath = Path.GetTempFileName();
        File.WriteAllText(vsPath, vsCode);
        File.WriteAllText(psPath, psCode);

        // Compila
        var vsBytecode = Compiler.CompileFromFile(vsPath, "VSMain", "vs_4_0");
        var psBytecode = Compiler.CompileFromFile(psPath, "PSMain", "ps_4_0");

        vertexShader = device.CreateVertexShader(vsBytecode.Span);
        pixelShader = device.CreatePixelShader(psBytecode.Span);

        inputLayout = device.CreateInputLayout(
            new[]
            {
            new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0),
            new InputElementDescription("TEXCOORD", 0, Format.R32G32_Float, 12, 0)
            },
            vsBytecode.Span
        );

        // Create constant buffer with image and screen resolution
        var bitmap = new System.Drawing.Bitmap(_imagePath);
        var imageWidth = (float)bitmap.Width;
        var imageHeight = (float)bitmap.Height;
        var screenWidth = (float)ClientSize.Width;
        var screenHeight = (float)ClientSize.Height;
        bitmap.Dispose();

        var resolutionData = new Vector4(imageWidth, imageHeight, screenWidth, screenHeight);

        var bufferDesc = new BufferDescription(
            bindFlags: BindFlags.ConstantBuffer,
            byteWidth: (uint)(sizeof(float) * 4),
            usage: ResourceUsage.Default
        );

        unsafe
        {
            var size = sizeof(float) * 4;
            var buffer = stackalloc float[4];
            buffer[0] = imageWidth;
            buffer[1] = imageHeight;
            buffer[2] = screenWidth;
            buffer[3] = screenHeight;

            var initData = new SubresourceData((IntPtr)buffer);
            resolutionBuffer = device.CreateBuffer(bufferDesc, initData);
        }


        // Cleaning temporary files
        File.Delete(vsPath);
        File.Delete(psPath);

        // Bind constant buffer to vertex shader slot 0
        context.VSSetConstantBuffer(0, resolutionBuffer);
    }

    // Sets up a fullscreen quad with flipped UVs (for top-left origin)
    private void SetupQuad()
    {
        float[] vertices = new float[]
        {
           // POSITION         // TEXCOORD (flip Y)
           -1f,  1f, 0f,       0f, 1f,  // top-left
            1f,  1f, 0f,       1f, 1f,  // top-right
           -1f, -1f, 0f,       0f, 0f,  // bottom-left
            1f, -1f, 0f,       1f, 0f   // bottom-right
        };

        var vertexBufferDesc = new BufferDescription(
            bindFlags: BindFlags.VertexBuffer,
            byteWidth: (uint)(sizeof(float) * vertices.Length),
            usage: ResourceUsage.Default,
            cpuAccessFlags: CpuAccessFlags.None,
            miscFlags: ResourceOptionFlags.None,
            structureByteStride: 0
        );

        unsafe
        {
            fixed (float* pVertices = vertices)
            {
                var vertexData = new SubresourceData((IntPtr)pVertices);
                vertexBuffer = device.CreateBuffer(vertexBufferDesc, vertexData);
            }
        }
    }

    // Main render loop — clears screen and draws textured quad
    private void RenderLoop(object sender, EventArgs e)
    {
        context.OMSetRenderTargets(rtv);
        context.ClearRenderTargetView(rtv, new Color4(0.1f, 0.1f, 0.1f, 1.0f)); // Clear to dark gray

        context.IASetInputLayout(inputLayout);
        context.IASetPrimitiveTopology(PrimitiveTopology.TriangleStrip);
        context.IASetVertexBuffer(0, vertexBuffer, sizeof(float) * 5);

        context.VSSetShader(vertexShader);
        context.PSSetShader(pixelShader);
        context.PSSetShaderResource(0, textureView);
        context.PSSetSampler(0, sampler);

        context.Draw(4, 0);
        swapChain.Present(1, PresentFlags.None); // Display the frame
    }
}
