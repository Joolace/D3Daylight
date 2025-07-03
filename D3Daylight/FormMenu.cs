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

        realms = new List<string>(Directory.GetDirectories("screenshots"));
        for (int i = 0; i < realms.Count; i++)
        {
            realms[i] = Path.GetFileName(realms[i]); // solo il nome della cartella
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
                var folderPath = Path.Combine("screenshots", selectedMap);
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