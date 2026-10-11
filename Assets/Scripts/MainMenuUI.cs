using UnityEngine;

// The main menu. Put on an empty object in the MainMenu scene.
public class MainMenuUI : MonoBehaviour
{
    private enum Page { Main, Levels, Settings, ConfirmNew }
    private Page page = Page.Main;

    private void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SaveManager.ApplySettings();
    }

    private void OnGUI()
    {
        UIKit.EnsureStyles();
        float s = UIKit.Scale;
        DrawBackground();

        float cx = Screen.width * 0.5f;
        UIKit.Title(new Rect(cx - 900 * s, 70 * s, 1800 * s, 200 * s), "ECHOES OF THE\nFORGOTTEN KINGDOM");

        switch (page)
        {
            case Page.Main: DrawMain(cx, s); break;
            case Page.Levels: DrawLevels(cx, s); break;
            case Page.Settings:
                if (SettingsUI.Draw(new Rect(cx - 450 * s, 300 * s, 900 * s, 640 * s))) page = Page.Main;
                break;
            case Page.ConfirmNew: DrawConfirmNew(cx, s); break;
        }
    }

    private void DrawMain(float cx, float s)
    {
        float w = 460 * s, h = 66 * s, gap = 16 * s;
        float y = 330 * s;
        UIKit.Panel(new Rect(cx - 280 * s, y - 25 * s, 560 * s, 5 * (h + gap) + 30 * s));

        if (UIKit.Button(new Rect(cx - w / 2, y, w, h), "Start Game"))
        {
            if (SaveManager.HasProgress) page = Page.ConfirmNew;
            else StartNewGame();
        }
        y += h + gap;

        bool old = GUI.enabled;
        GUI.enabled = SaveManager.HasProgress;
        if (UIKit.Button(new Rect(cx - w / 2, y, w, h), "Continue")) Continue();
        GUI.enabled = old;
        y += h + gap;

        if (UIKit.Button(new Rect(cx - w / 2, y, w, h), "Level Selection")) page = Page.Levels;
        y += h + gap;

        if (UIKit.Button(new Rect(cx - w / 2, y, w, h), "Settings")) page = Page.Settings;
        y += h + gap;

        if (UIKit.Button(new Rect(cx - w / 2, y, w, h), "Exit Game")) QuitGame();
    }

    private void DrawLevels(float cx, float s)
    {
        float w = 640 * s, h = 66 * s, gap = 16 * s;
        float y = 330 * s;
        int count = LevelManager.LevelCount;
        UIKit.Panel(new Rect(cx - 380 * s, y - 25 * s, 760 * s, (count + 1) * (h + gap) + 30 * s));

        for (int i = 1; i <= count; i++)
        {
            bool unlocked = i <= SaveManager.UnlockedLevel;
            bool available = LevelManager.IsLevelAvailable(i);

            string label = "Level " + i + ": " + LevelManager.LevelTitles[i - 1];
            if (!unlocked) label += "  (Locked)";
            else if (!available) label += "  (Coming soon)";
            else if (SaveManager.IsCompleted(i)) label += "  (Completed)";

            bool old = GUI.enabled;
            GUI.enabled = unlocked && available;
            if (UIKit.Button(new Rect(cx - w / 2, y, w, h), label)) LevelManager.LoadLevel(i);
            GUI.enabled = old;
            y += h + gap;
        }

        if (UIKit.Button(new Rect(cx - w / 2, y, w, h), "Back")) page = Page.Main;
    }

    private void DrawConfirmNew(float cx, float s)
    {
        Rect panel = new Rect(cx - 400 * s, 340 * s, 800 * s, 380 * s);
        UIKit.Panel(panel);
        UIKit.Center(new Rect(panel.x + 40 * s, panel.y + 40 * s, panel.width - 80 * s, 160 * s),
                     "Starting a new game erases your saved progress. Are you sure?");

        if (UIKit.Button(new Rect(cx - 330 * s, panel.yMax - 120 * s, 300 * s, 66 * s), "Yes, start over")) StartNewGame();
        if (UIKit.Button(new Rect(cx + 30 * s, panel.yMax - 120 * s, 300 * s, 66 * s), "Cancel")) page = Page.Main;
    }

    private void StartNewGame()
    {
        SaveManager.ResetProgress();
        LevelManager.LoadLevel(1);
    }

    private void Continue()
    {
        for (int level = SaveManager.UnlockedLevel; level >= 1; level--)
        {
            if (LevelManager.IsLevelAvailable(level))
            {
                LevelManager.LoadLevel(level);
                return;
            }
        }
        Debug.LogWarning("MainMenuUI: no playable level found in the build settings.");
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private static void DrawBackground()
    {
        Color top = new Color(0.04f, 0.03f, 0.10f);
        Color bottom = new Color(0.22f, 0.10f, 0.30f);
        int strips = 48;
        float h = Screen.height / (float)strips;
        for (int i = 0; i < strips; i++)
            UIKit.DrawRect(new Rect(0, i * h, Screen.width, h + 1f), Color.Lerp(top, bottom, i / (float)(strips - 1)));
    }
}
