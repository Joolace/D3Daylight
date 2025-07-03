using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

public class FormMenu : Form
{
    private ListBox mapList;
    private Button startButton;
    private string selectedMap;

    private List<string> maps = new()
    {
        "Azarov's Resting Place", "Badham Preschool", "Blood Lodge", "Coal Tower",
        "Dead Dawg Saloon", "Disturbed Ward", "Eyrie of Crows", "Family Residence",
        "Father Campbell's Chapel", "Forgotten Ruins", "Fractured Cowshed",
        "Freddy Fazbear's Pizza", "Garden of Joy", "Gas Heaven", "Greenville Square",
        "Grim Pantry", "Groaning Storehouse", "Ironworks of Misery", "Lampkin Lane",
        "Midwich Elementary School", "Mother's Dwelling", "Mount Ormond Resort",
        "Nostromo Wreckage", "Raccoon City Police Station", "Raccoon City Police Station East Wing",
        "Raccoon City Police Station West Wing", "Rancid Abattoir", "Rotten Fields",
        "Sanctum of Wrath", "Shelter Woods", "Suffocation Pit", "The Game",
        "The Pale Rose", "The Shattered Square", "The Temple of Purgation",
        "The Thompson House", "The Underground Complex", "Toba Landing",
        "Torment Creek", "Treatment Theatre", "Wreckers' Yard", "Wretched Shop"
    };

    public FormMenu()
    {
        Text = "Select a map";
        Width = 400;
        Height = 600;
        StartPosition = FormStartPosition.CenterScreen;

        mapList = new ListBox
        {
            Dock = DockStyle.Fill
        };
        mapList.Items.AddRange(maps.ToArray());
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
                var imagePath = Path.Combine("screenshots", selectedMap + ".png");
                if (File.Exists(imagePath))
                {
                    Hide();
                    new D3DRenderer(imagePath).ShowDialog();
                    Show();
                }
                else
                {
                    MessageBox.Show($"Screenshot not found for {selectedMap}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        };

        Controls.Add(mapList);
        Controls.Add(startButton);
    }
}
