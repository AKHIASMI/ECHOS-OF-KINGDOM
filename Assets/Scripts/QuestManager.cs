using System.Collections.Generic;
using UnityEngine;

// Tracks the objectives of the level and decides when the level is complete.
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [SerializeField] private int crystalsRequired = 3;

    private class Objective
    {
        public string id;
        public string text;
        public bool done;
    }

    private readonly List<Objective> objectives = new List<Objective>();

    public bool LevelComplete { get; private set; }

    public string CurrentObjective
    {
        get
        {
            foreach (Objective o in objectives)
                if (!o.done) return o.text;
            return "All objectives complete!";
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        objectives.Add(new Objective { id = "crystals", text = "Collect " + crystalsRequired + " crystals" });
        objectives.Add(new Objective { id = "gate", text = "Open the south gate at the ancient switch" });
        objectives.Add(new Objective { id = "puzzle", text = "Solve the symbol puzzle (read the tablet)" });
        objectives.Add(new Objective { id = "portal", text = "Enter the magic portal" });
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (!IsDone("crystals") && MagicSystem.Instance != null
            && MagicSystem.Instance.CrystalsCollected >= crystalsRequired)
        {
            Complete("crystals");
            MagicSystem.Instance.PostMessage("Objective complete: crystals collected!");
        }
    }

    public bool IsDone(string id)
    {
        foreach (Objective o in objectives)
            if (o.id == id) return o.done;
        return false;
    }

    public void Complete(string id)
    {
        foreach (Objective o in objectives)
        {
            if (o.id != id) continue;
            o.done = true;
            break;
        }
        CheckLevelComplete();
    }

    // True when every objective except the given one is finished.
    public bool AllDoneExcept(string id)
    {
        foreach (Objective o in objectives)
            if (o.id != id && !o.done) return false;
        return true;
    }

    private void CheckLevelComplete()
    {
        if (LevelComplete) return;
        foreach (Objective o in objectives)
            if (!o.done) return;

        LevelComplete = true;

        // Save progress
        int level = LevelManager.CurrentLevelIndex;
        int crystals = MagicSystem.Instance != null ? MagicSystem.Instance.CrystalsCollected : 0;
        if (level > 0) SaveManager.CompleteLevel(level, crystals);

        FreezePlayer();
    }

    private void FreezePlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null) pc.CanMove = false;
        }

        if (Camera.main != null)
        {
            CameraController cam = Camera.main.GetComponent<CameraController>();
            if (cam != null) cam.enabled = false;
        }
        CameraController.SetCursorLocked(false);
    }
}
