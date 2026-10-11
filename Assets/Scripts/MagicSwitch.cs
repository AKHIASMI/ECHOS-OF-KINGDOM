using UnityEngine;

// An ancient switch. Costs magic energy, then opens a linked door.
public class MagicSwitch : Interactable
{
    public DoorController door;
    public Renderer indicator;
    public float energyCost = 30f;

    private bool activated;

    public override bool CanInteract => !activated;

    public override string GetPrompt()
    {
        return "Press E - Activate ancient switch (" + energyCost + " energy)";
    }

    public override void Interact()
    {
        if (activated) return;

        if (door == null)
        {
            Debug.LogError("MagicSwitch: no door assigned on " + name);
            return;
        }

        MagicSystem magic = MagicSystem.Instance;
        if (magic == null)
        {
            Debug.LogError("MagicSwitch: no MagicSystem in the scene.");
            return;
        }

        if (!magic.TrySpend(energyCost))
        {
            magic.PostMessage("Not enough magic energy. Find more crystals!");
            return;
        }

        activated = true;
        door.Open();
        magic.PostMessage("The ancient gate opens!");

        if (QuestManager.Instance != null) QuestManager.Instance.Complete("gate");
        if (indicator != null) MaterialGlow.Apply(indicator, new Color(0.2f, 1f, 0.4f), 2f);
    }
}
