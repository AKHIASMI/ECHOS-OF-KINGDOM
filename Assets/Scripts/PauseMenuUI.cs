using UnityEngine;

// Pause menu, Game Over screen and Level Completed screen. One per level scene.
public class PauseMenuUI : MonoBehaviour
{
    private enum Page { Pause, Settings, ConfirmRestart, ConfirmMenu }
    private Page page = Page.Pause;

    private void OnGUI()
    {
        GUI.depth = -10; // draw on top of the HUD

        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        UIKit.EnsureStyles();
        float s = UIKit.Scale;
        float cx = Screen.width * 0.5f;

        QuestManager quest = QuestManager.Instance;
        if (quest != null && quest.LevelComplete)
        {
            DrawLevelComplete(cx, s);
            return;
        }

        if (gm.IsGameOver)
        {
            DrawGameOver(cx, s);
            return;
        }

        if (!gm.IsPaused)
        {
            page = Page.Pause;
            return;
        }

        UIKit.Dim(0.6f);
        switch (page)
        {
            case Page.Pause: DrawPause(gm, cx, s); break;
            case Page.Settings:
                if (SettingsUI.Draw(new Rect(cx - 450 * s, 190 * s, 900 * s, 640 * s))) page = Page.Pause;
                break;
            case Page.ConfirmRestart:
                DrawConfirm(cx, s, "Restart this level? Your progress in this level will be lost.",
                            "Yes, restart", () => LevelManager.RestartCurrent());
                break;
            case Page.ConfirmMenu:
                DrawConfirm(cx, s, "Return to the main menu? Progress in this level will be lost.",
                            "Yes, quit to menu", () => LevelManager.LoadMainMenu());
                break;
        }
    }

    private void DrawPause(GameManager gm, float cx, float s)
    {
        float w = 420 * s, h = 66 * s, gap = 16 * s;
        Rect panel = new Rect(cx - 280 * s, 230 * s, 560 * s, 130 * s + 4 * (h + gap) + 20 * s);
        UIKit.Panel(panel);
        UIKit.Header(new Rect(panel.x, panel.y + 20 * s, panel.width, 70 * s), "PAUSED");

        float y = panel.y + 120 * s;
        if (UIKit.Button(new Rect(cx - w / 2, y, w, h), "Resume")) gm.Resume();
        y += h + gap;
        if (UIKit.Button(new Rect(cx - w / 2, y, w, h), "Restart Level")) page = Page.ConfirmRestart;
        y += h + gap;
        if (UIKit.Button(new Rect(cx - w / 2, y, w, h), "Settings")) page = Page.Settings;
        y += h + gap;
        if (UIKit.Button(new Rect(cx - w / 2, y, w, h), "Main Menu")) page = Page.ConfirmMenu;
    }

    private void DrawConfirm(float cx, float s, string message, string yesText, System.Action onYes)
    {
        Rect panel = new Rect(cx - 400 * s, 300 * s, 800 * s, 380 * s);
        UIKit.Panel(panel);
        UIKit.Center(new Rect(panel.x + 40 * s, panel.y + 40 * s, panel.width - 80 * s, 160 * s), message);

        if (UIKit.Button(new Rect(cx - 330 * s, panel.yMax - 120 * s, 300 * s, 66 * s), yesText)) onYes();
        if (UIKit.Button(new Rect(cx + 30 * s, panel.yMax - 120 * s, 300 * s, 66 * s), "Cancel")) page = Page.Pause;
    }

    private void DrawGameOver(float cx, float s)
    {
        UIKit.Dim(0.75f);
        Rect panel = new Rect(cx - 330 * s, 260 * s, 660 * s, 460 * s);
        UIKit.Panel(panel);
        UIKit.Title(new Rect(panel.x, panel.y + 30 * s, panel.width, 130 * s), "GAME OVER");
        UIKit.Center(new Rect(panel.x + 40 * s, panel.y + 170 * s, panel.width - 80 * s, 60 * s),
                     "The curse has claimed you...");

        if (UIKit.Button(new Rect(cx - 210 * s, panel.y + 250 * s, 420 * s, 66 * s), "Retry")) LevelManager.RestartCurrent();
        if (UIKit.Button(new Rect(cx - 210 * s, panel.y + 332 * s, 420 * s, 66 * s), "Main Menu")) LevelManager.LoadMainMenu();
    }

    private void DrawLevelComplete(float cx, float s)
    {
        UIKit.Dim(0.75f);
        Rect panel = new Rect(cx - 380 * s, 200 * s, 760 * s, 600 * s);
        UIKit.Panel(panel);
        UIKit.Title(new Rect(panel.x, panel.y + 30 * s, panel.width, 130 * s), "LEVEL COMPLETED!");

        MagicSystem magic = MagicSystem.Instance;
        string stats = magic != null ? "Crystals collected: " + magic.CrystalsCollected + " / " + magic.TotalCrystals : "";
        UIKit.Center(new Rect(panel.x + 40 * s, panel.y + 170 * s, panel.width - 80 * s, 60 * s), stats);

        float y = panel.y + 260 * s;
        float w = 460 * s, h = 66 * s, gap = 16 * s;

        if (LevelManager.HasNextLevel)
        {
            if (UIKit.Button(new Rect(cx - w / 2, y, w, h), "Next Level")) LevelManager.LoadNext();
        }
        else
        {
            UIKit.Center(new Rect(cx - w / 2, y, w, h), "More levels coming soon!");
        }
        y += h + gap;
        if (UIKit.Button(new Rect(cx - w / 2, y, w, h), "Replay Level")) LevelManager.RestartCurrent();
        y += h + gap;
        if (UIKit.Button(new Rect(cx - w / 2, y, w, h), "Main Menu")) LevelManager.LoadMainMenu();
    }
}
