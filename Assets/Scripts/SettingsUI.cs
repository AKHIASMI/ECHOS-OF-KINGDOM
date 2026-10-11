using UnityEngine;

// The settings panel used by both the main menu and the pause menu.
public static class SettingsUI
{
    // Draws the panel. Returns true when the Back button was pressed.
    public static bool Draw(Rect area)
    {
        float s = UIKit.Scale;
        UIKit.Panel(area);
        UIKit.Header(new Rect(area.x, area.y + 20 * s, area.width, 70 * s), "Settings");

        float x = area.x + 70 * s;
        float w = area.width - 140 * s;
        float y = area.y + 120 * s;

        // Music volume
        UIKit.Label(new Rect(x, y, w, 36 * s), "Music Volume: " + Mathf.RoundToInt(SaveManager.MusicVolume * 100f) + "%");
        float music = GUI.HorizontalSlider(new Rect(x, y + 44 * s, w, 24 * s), SaveManager.MusicVolume, 0f, 1f);
        if (!Mathf.Approximately(music, SaveManager.MusicVolume)) SaveManager.MusicVolume = music;

        // Sound effects volume
        y += 100 * s;
        UIKit.Label(new Rect(x, y, w, 36 * s), "Sound Effects Volume: " + Mathf.RoundToInt(SaveManager.SfxVolume * 100f) + "%");
        float sfx = GUI.HorizontalSlider(new Rect(x, y + 44 * s, w, 24 * s), SaveManager.SfxVolume, 0f, 1f);
        if (!Mathf.Approximately(sfx, SaveManager.SfxVolume)) SaveManager.SfxVolume = sfx;

        // Graphics quality
        y += 100 * s;
        string[] names = QualitySettings.names;
        if (names.Length > 0)
        {
            int q = SaveManager.QualityLevel;
            UIKit.Label(new Rect(x, y, w, 36 * s), "Graphics Quality");
            if (UIKit.Button(new Rect(x, y + 40 * s, 70 * s, 56 * s), "<"))
                SaveManager.QualityLevel = Mathf.Max(0, q - 1);
            UIKit.Center(new Rect(x + 80 * s, y + 40 * s, w - 160 * s, 56 * s), names[q]);
            if (UIKit.Button(new Rect(x + w - 70 * s, y + 40 * s, 70 * s, 56 * s), ">"))
                SaveManager.QualityLevel = Mathf.Min(names.Length - 1, q + 1);
        }

        if (UIKit.Button(new Rect(area.center.x - 150 * s, area.yMax - 90 * s, 300 * s, 60 * s), "Back"))
        {
            SaveManager.Save();
            return true;
        }
        return false;
    }
}
