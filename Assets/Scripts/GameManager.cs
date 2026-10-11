using UnityEngine;

// One per level scene. Handles pause (Esc), game over, and cursor/time state.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool IsPaused { get; private set; }
    public bool IsGameOver { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Time.timeScale = 1f;
        SaveManager.ApplySettings();
    }

    private void Start()
    {
        SaveManager.MarkStarted();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        Time.timeScale = 1f; // never leave the next scene frozen
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        if (IsGameOver) return;
        if (QuestManager.Instance != null && QuestManager.Instance.LevelComplete) return;

        if (IsPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        CameraController.SetCursorLocked(false);
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        CameraController.SetCursorLocked(true);
    }

    public void GameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        IsPaused = false;
        Time.timeScale = 0f;
        CameraController.SetCursorLocked(false);
    }
}
