using UnityEngine;

// Reading the tablet shows the puzzle hint.
public class AncientTablet : Interactable
{
    public PuzzleManager puzzle;

    public override string GetPrompt()
    {
        return "Press E - Read the ancient tablet";
    }

    public override void Interact()
    {
        if (puzzle == null || MagicSystem.Instance == null) return;
        MagicSystem.Instance.PostMessage(puzzle.GetHint(), 7f);
    }
}
