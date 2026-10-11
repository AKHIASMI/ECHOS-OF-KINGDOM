using UnityEngine;
using TMPro;

public class LevelTimer : MonoBehaviour
{
    public float timeLimitSeconds = 120f;
    public TextMeshProUGUI timerText;

    float remaining;
    bool finished;

    void Start()
    {
        remaining = timeLimitSeconds;
        UpdateText();
    }

    void Update()
    {
        if (finished) return;

        if (QuestManager.Instance != null && QuestManager.Instance.LevelComplete) { finished = true; return; }
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) { finished = true; return; }

        remaining -= Time.deltaTime;
        if (remaining <= 0f)
        {
            remaining = 0f;
            finished = true;
            UpdateText();
            if (GameManager.Instance != null) GameManager.Instance.GameOver();
            return;
        }
        UpdateText();
    }

    void UpdateText()
    {
        if (timerText == null) return;
        int m = Mathf.FloorToInt(remaining / 60f);
        int s = Mathf.FloorToInt(remaining % 60f);
        timerText.text = "Time: " + m.ToString("00") + ":" + s.ToString("00");
        timerText.color = remaining <= 30f ? Color.red : Color.white;
    }
}