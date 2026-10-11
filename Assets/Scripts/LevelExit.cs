using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    public string menuSceneName = "MainMenu";

    bool loading;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Exit touched by: " + other.name);

        if (loading) return;
        if (!other.CompareTag("Player") && !other.transform.root.CompareTag("Player")) return;

        if (GateObjective.Instance == null || !GateObjective.Instance.IsOpen)
        {
            Debug.Log("Gate is not open yet");
            return;
        }

        Debug.Log("Loading scene: " + menuSceneName);
        loading = true;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(menuSceneName);
    }
}