using UnityEngine;

// Shared fantasy-style look for all menus and the HUD (drawn with OnGUI).
// Call UIKit.EnsureStyles() at the start of every OnGUI that uses it.
public static class UIKit
{
    public static readonly Color Gold = new Color(1f, 0.85f, 0.45f);

    private static GUIStyle titleStyle, headerStyle, buttonStyle, labelStyle, centerStyle;
    private static Texture2D texNormal, texHover, texActive, texPanel;

    public static float Scale => Screen.height / 1080f;

    public static void EnsureStyles()
    {
        if (texNormal == null || texHover == null || texActive == null || texPanel == null)
        {
            texNormal = MakeTex(new Color(0.16f, 0.12f, 0.22f, 0.95f));
            texHover = MakeTex(new Color(0.35f, 0.24f, 0.5f, 1f));
            texActive = MakeTex(new Color(0.55f, 0.4f, 0.2f, 1f));
            texPanel = MakeTex(new Color(0.07f, 0.05f, 0.12f, 0.92f));
            buttonStyle = null;
        }

        if (buttonStyle == null)
        {
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                border = new RectOffset(0, 0, 0, 0)
            };
            SetButtonState(buttonStyle.normal, texNormal, Color.white);
            SetButtonState(buttonStyle.hover, texHover, Gold);
            SetButtonState(buttonStyle.active, texActive, Color.white);
            SetButtonState(buttonStyle.focused, texNormal, Color.white);
            SetButtonState(buttonStyle.onNormal, texNormal, Color.white);
            SetButtonState(buttonStyle.onHover, texHover, Gold);
            SetButtonState(buttonStyle.onActive, texActive, Color.white);
            SetButtonState(buttonStyle.onFocused, texNormal, Color.white);

            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.normal.textColor = Color.white;

            centerStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, wordWrap = true };
            centerStyle.normal.textColor = Color.white;

            headerStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            headerStyle.normal.textColor = Gold;

            titleStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, wordWrap = true };
            titleStyle.normal.textColor = Gold;
        }

        float s = Scale;
        buttonStyle.fontSize = Mathf.RoundToInt(28 * s);
        labelStyle.fontSize = Mathf.RoundToInt(22 * s);
        centerStyle.fontSize = Mathf.RoundToInt(24 * s);
        headerStyle.fontSize = Mathf.RoundToInt(44 * s);
        titleStyle.fontSize = Mathf.RoundToInt(72 * s);
    }

    private static void SetButtonState(GUIStyleState state, Texture2D bg, Color text)
    {
        state.background = bg;
        state.textColor = text;
    }

    private static Texture2D MakeTex(Color c)
    {
        Texture2D t = new Texture2D(1, 1);
        t.SetPixel(0, 0, c);
        t.Apply();
        t.hideFlags = HideFlags.HideAndDontSave;
        return t;
    }

    public static void DrawRect(Rect r, Color c)
    {
        Color old = GUI.color;
        GUI.color = c;
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = old;
    }

    public static void Dim(float alpha)
    {
        DrawRect(new Rect(0, 0, Screen.width, Screen.height), new Color(0f, 0f, 0f, alpha));
    }

    public static void Panel(Rect r)
    {
        float s = Scale;
        GUI.DrawTexture(r, texPanel);
        DrawRect(new Rect(r.x, r.y, r.width, 3 * s), Gold);
        DrawRect(new Rect(r.x, r.yMax - 3 * s, r.width, 3 * s), Gold);
    }

    public static bool Button(Rect r, string text) { return GUI.Button(r, text, buttonStyle); }
    public static void Label(Rect r, string text) { GUI.Label(r, text, labelStyle); }
    public static void Center(Rect r, string text) { GUI.Label(r, text, centerStyle); }
    public static void Header(Rect r, string text) { GUI.Label(r, text, headerStyle); }
    public static void Title(Rect r, string text) { GUI.Label(r, text, titleStyle); }
}
