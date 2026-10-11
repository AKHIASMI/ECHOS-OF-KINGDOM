using UnityEngine;
using UnityEngine.SceneManagement;

// Scene names and level loading. Scenes must be listed in Build Profiles (the setup tool does this).
public static class LevelManager
{
    public const string MainMenuScene = "MainMenu";

    public static readonly string[] SceneNames =
{
        "Level1_ForgottenCourtyard",
        "Level2_RuinedSanctuary",
        "Level3_CastleOutside",
        "Level4_RoyalChamber"
    };

    public static readonly string[] LevelTitles =
    {
        "Forgotten Courtyard",
        "Ruined Sanctuary",
        "Shadow Castle",
        "The Royal Chamber"
    };

    public static int LevelCount => SceneNames.Length;

    // 1-based index of the current level, or 0 when not in a level scene.
    public static int CurrentLevelIndex
    {
        get
        {
            string current = SceneManager.GetActiveScene().name;
            for (int i = 0; i < SceneNames.Length; i++)
                if (SceneNames[i] == current) return i + 1;
            return 0;
        }
    }

    public static bool IsLevelAvailable(int level)
    {
        if (level < 1 || level > SceneNames.Length) return false;
        return SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/" + SceneNames[level - 1] + ".unity") >= 0;
    }

    public static bool HasNextLevel => IsLevelAvailable(CurrentLevelIndex + 1);

    public static void LoadLevel(int level)
    {
        if (!IsLevelAvailable(level))
        {
            Debug.LogWarning("LevelManager: level " + level + " is not in the build settings.");
            return;
        }
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneNames[level - 1]);
    }

    public static void LoadNext()
    {
        LoadLevel(CurrentLevelIndex + 1);
    }

    public static void RestartCurrent()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public static void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(MainMenuScene);
    }
}
