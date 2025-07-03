using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

public class FormMenu : Form
{
    private ListBox mapList;
    private Button startButton;
    private string selectedMap;

    private List<string> realms;

    public FormMenu()
    {
        Text = "Select a realm";
        Width = 400;
        Height = 600;
        StartPosition = FormStartPosition.CenterScreen;

        mapList = new ListBox
        {
            Dock = DockStyle.Fill
        };

        // Build full path to the screenshots folder (relative to executable path)
        string screenshotsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "screenshots");

        // Check if it exists — if not, show error
        if (!Directory.Exists(screenshotsFolder))
        {
            MessageBox.Show($"The 'screenshots' folder was not found at:\n{screenshotsFolder}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(1);
        }

        // Load subfolders as realm names
        realms = new List<string>(Directory.GetDirectories(screenshotsFolder));
        for (int i = 0; i < realms.Count; i++)
        {
            realms[i] = Path.GetFileName(realms[i]); // Extract just the folder name
        }


        mapList.Items.AddRange(realms.ToArray());
        mapList.SelectedIndexChanged += (s, e) =>
        {
            selectedMap = mapList.SelectedItem.ToString();
            startButton.Enabled = true;
        };

        startButton = new Button
        {
            Text = "Start preview",
            Dock = DockStyle.Bottom,
            Height = 40,
            Enabled = false
        };

        startButton.Click += (s, e) =>
        {
            if (!string.IsNullOrEmpty(selectedMap))
            {
                var folderPath = Path.Combine(screenshotsFolder, selectedMap);
                if (Directory.Exists(folderPath))
                {
                    Hide();
                    new D3DRenderer(folderPath).ShowDialog();
                    Show();
                }
                else
                {
                    MessageBox.Show($"Folder not found: {folderPath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        };

        Controls.Add(mapList);
        Controls.Add(startButton);
    }
}