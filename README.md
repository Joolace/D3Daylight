
# D3Daylight

A Windows desktop application built with Direct3D 11 and WinForms that previews **Dead by Daylight** map screenshots in real time using GPU acceleration. Fully compatible with **ReShade**.

## 📸 Features

- Real-time display of PNG map screenshots
- GPU-accelerated rendering via Direct3D 11
- Maintains original aspect ratio during window resize
- ReShade-compatible rendering pipeline
- Simple Windows Forms UI for selecting maps

## 🛠️ Requirements

### Runtime

- **Operating System**: Windows 10 or 11 (x64)
- **.NET Runtime**: [.NET 8.0 or newer](https://dotnet.microsoft.com/download/dotnet/8.0)
- **DirectX**: Direct3D 11-compatible GPU and drivers
- **VC++ Redistributable**: [2015–2022 x64](https://aka.ms/vs/17/release/vc_redist.x64.exe)

### Development (Optional)

- Visual Studio 2022 or later
- .NET SDK 8+
- NuGet packages:
  - `Vortice.Windows`
  - `SharpGen.Runtime`

## 📁 Folder Structure

Place your map screenshots in the `screenshots/` folder using the exact names provided in the list:

```bash
screenshots/
├── Azarov's Resting Place.png
├── Blood Lodge.png
├── Dead Dawg Saloon.png
└── etc.
```

## 🚀 How to Run

1. Clone the repository:

   ```bash
   git clone https://github.com/your-username/D3Daylight.git
   cd D3Daylight
   ```

2. Add your `.png` screenshots to the `screenshots/` folder.

3. Open the solution in Visual Studio and run the project.

## 📦 Publishing

To publish a standalone `.exe`:

```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

This creates a self-contained executable that runs without requiring .NET pre-installed.

## 🖼️ Screenshot

```markdown
SOON
```

## 📘 License

This project is licensed under the [GPL-3.0 license](https://github.com/Joolace/D3Daylight/tree/master?tab=GPL-3.0-1-ov-file).

## 🙏 Credits

- [Vortice.Windows](https://github.com/amerkoleci/Vortice.Windows)
- [SharpGenTools](https://github.com/SharpGenTools/SharpGenTools)
- [ReShade](https://github.com/crosire/reshade)
