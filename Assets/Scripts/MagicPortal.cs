using UnityEngine;

// The level exit. Dormant until the puzzle is solved; completes the level when entered.
[RequireComponent(typeof(Collider))]
public class MagicPortal : MonoBehaviour
{
    public Renderer visual;

    private bool active;

    private void Start()
    {
        MaterialGlow.Apply(visual, new Color(0.25f, 0.25f, 0.3f), 0f);
    }

    public void Activate()
    {
        active = true;
        MaterialGlow.Apply(visual, new Color(0.4f, 0.8f, 1f), 3f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        MagicSystem magic = MagicSystem.Instance;
        QuestManager quest = QuestManager.Instance;

        if (!active)
        {
            if (magic != null) magic.PostMessage("The portal is dormant. Solve the symbol puzzle first.");
            return;
        }

        if (quest != null && !quest.AllDoneExcept("portal"))
        {
            if (magic != null) magic.PostMessage("Finish your other objectives first: " + quest.CurrentObjective, 4f);
            return;
        }

        if (quest != null) quest.Complete("portal");
    }
}
