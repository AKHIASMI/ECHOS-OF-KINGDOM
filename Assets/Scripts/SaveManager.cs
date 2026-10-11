using UnityEngine;

// Saves progress and settings between sessions using PlayerPrefs.
public static class SaveManager
{
    private const string KeyStarted = "echoes_started";
    private const string KeyUnlocked = "echoes_unlocked";
    private const string KeyCompleted = "echoes_completed_";
    private const string KeyBestCrystals = "echoes_crystals_";
    private const string KeyMusic = "echoes_music";
    private const string KeySfx = "echoes_sfx";
    private const string KeyQuality = "echoes_quality";

    public static bool HasProgress => PlayerPrefs.GetInt(KeyStarted, 0) == 1;
    public static int UnlockedLevel => Mathf.Max(1, PlayerPrefs.GetInt(KeyUnlocked, 1));

    public static void MarkStarted()
    {
        if (HasProgress) return;
        PlayerPrefs.SetInt(KeyStarted, 1);
        PlayerPrefs.Save();
    }

    public static bool IsCompleted(int level)
    {
        return PlayerPrefs.GetInt(KeyCompleted + level, 0) == 1;
    }

    public static int BestCrystals(int level)
    {
        return PlayerPrefs.GetInt(KeyBestCrystals + level, 0);
    }

    public static void CompleteLevel(int level, int crystals)
    {
        PlayerPrefs.SetInt(KeyStarted, 1);
        PlayerPrefs.SetInt(KeyCompleted + level, 1);
        if (crystals > BestCrystals(level)) PlayerPrefs.SetInt(KeyBestCrystals + level, crystals);
        if (level + 1 > UnlockedLevel) PlayerPrefs.SetInt(KeyUnlocked, level + 1);
        PlayerPrefs.Save();
    }

    // Clears story progress only. Settings are kept.
    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(KeyStarted);
        PlayerPrefs.DeleteKey(KeyUnlocked);
        for (int i = 1; i <= 10; i++)
        {
            PlayerPrefs.DeleteKey(KeyCompleted + i);
            PlayerPrefs.DeleteKey(KeyBestCrystals + i);
        }
        PlayerPrefs.Save();
    }

    // ----- Settings -----
    public static float MusicVolume
    {
        get => PlayerPrefs.GetFloat(KeyMusic, 0.7f);
        set => PlayerPrefs.SetFloat(KeyMusic, Mathf.Clamp01(value));
    }

    public static float SfxVolume
    {
        get => PlayerPrefs.GetFloat(KeySfx, 0.8f);
        set => PlayerPrefs.SetFloat(KeySfx, Mathf.Clamp01(value));
    }

    public static int QualityLevel
    {
        get
        {
            int max = Mathf.Max(0, QualitySettings.names.Length - 1);
            return Mathf.Clamp(PlayerPrefs.GetInt(KeyQuality, QualitySettings.GetQualityLevel()), 0, max);
        }
        set
        {
            PlayerPrefs.SetInt(KeyQuality, value);
            QualitySettings.SetQualityLevel(value, true);
        }
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }

    public static void ApplySettings()
    {
        if (PlayerPrefs.HasKey(KeyQuality) && QualitySettings.names.Length > 0)
            QualitySettings.SetQualityLevel(QualityLevel, true);
    }
}
