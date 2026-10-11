using UnityEngine;
using TMPro;

public class ManualPanel : MonoBehaviour
{
    public GameObject panel;
    public TextMeshProUGUI bodyText;
    public KeyCode reopenKey = KeyCode.H;

    [TextArea(8, 20)]
    public string manualText =
        "HOW TO PLAY\n\n" +
        "BLUE = SAFE. Enemies cannot hurt you inside blue zones.\n" +
        "RED = CURSED. Red zones damage you. Stay out of them.\n" +
        "CRYSTALS: collect them to complete the level's goal.\n" +
        "POWER ENERGY: powers your magic. Use it wisely.\n" +
        "GATE: opens when you finish the level's goal.\n\n" +
        "Press H to open this guide again.";

    void Start()
    {
        if (bodyText != null) bodyText.text = manualText;
        Show();
    }

    void Update()
    {
        if (Input.GetKeyDown(reopenKey))
        {
            if (panel.activeSelf) Close(); else Show();
        }
    }

    public void Show()
    {
        panel.SetActive(true);
        Time.timeScale = 0f;
        CameraController.SetCursorLocked(false);
    }

    public void Close()
    {
        panel.SetActive(false);
        Time.timeScale = 1f;
        CameraController.SetCursorLocked(true);
    }
}// JavaScript source code
