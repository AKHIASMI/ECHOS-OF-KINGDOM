using UnityEngine;

// In-game HUD: level name, objective, health, magic energy, crystals, messages, interaction prompt.
public class HUDController : MonoBehaviour
{
    [SerializeField] private string levelName = "Level 1: Forgotten Courtyard";

    private void OnGUI()
    {
        MagicSystem magic = MagicSystem.Instance;
        if (magic == null) return;

        UIKit.EnsureStyles();
        float s = UIKit.Scale;

        QuestManager quest = QuestManager.Instance;
        string objective = quest != null ? quest.CurrentObjective : "Explore the courtyard";

        // Panel
        UIKit.Panel(new Rect(15 * s, 15 * s, 640 * s, 245 * s));
        UIKit.Label(new Rect(30 * s, 22 * s, 610 * s, 30 * s), levelName);
        UIKit.Label(new Rect(30 * s, 52 * s, 610 * s, 30 * s), "Objective: " + objective);

        // Health bar
        PlayerHealth health = PlayerHealth.Instance;
        float hp = health != null ? health.Health : 0f;
        float hpMax = health != null ? health.MaxHealth : 1f;
        UIKit.Label(new Rect(30 * s, 88 * s, 440 * s, 30 * s), "Health  " + Mathf.CeilToInt(hp) + " / " + Mathf.CeilToInt(hpMax));
        DrawBar(new Rect(30 * s, 120 * s, 420 * s, 18 * s), hpMax > 0f ? hp / hpMax : 0f, new Color(0.85f, 0.2f, 0.2f));

        // Magic energy bar
        UIKit.Label(new Rect(30 * s, 144 * s, 440 * s, 30 * s), "Magic Energy");
        float fill = magic.MaxEnergy > 0f ? magic.Energy / magic.MaxEnergy : 0f;
        DrawBar(new Rect(30 * s, 176 * s, 420 * s, 18 * s), fill, new Color(0.3f, 0.8f, 1f));

        UIKit.Label(new Rect(30 * s, 202 * s, 440 * s, 30 * s),
                    "Crystals: " + magic.CrystalsCollected + " / " + magic.TotalCrystals);

        // Message
        if (magic.MessageActive)
        {
            Rect r = new Rect(Screen.width * 0.5f - 480 * s, Screen.height - 170 * s, 960 * s, 54 * s);
            UIKit.DrawRect(r, new Color(0f, 0f, 0f, 0.65f));
            UIKit.Center(r, magic.Message);
        }

        // Interaction prompt
        PlayerInteraction interaction = PlayerInteraction.Instance;
        bool levelDone = quest != null && quest.LevelComplete;
        if (interaction != null && interaction.Current != null && !levelDone)
        {
            Rect r = new Rect(Screen.width * 0.5f - 400 * s, Screen.height - 105 * s, 800 * s, 44 * s);
            UIKit.DrawRect(r, new Color(0f, 0f, 0f, 0.6f));
            UIKit.Center(r, interaction.Current.GetPrompt());
        }
    }

    private static void DrawBar(Rect bar, float fill, Color color)
    {
        UIKit.DrawRect(bar, new Color(0.1f, 0.1f, 0.15f, 0.9f));
        UIKit.DrawRect(new Rect(bar.x, bar.y, bar.width * Mathf.Clamp01(fill), bar.height), color);
    }
}
