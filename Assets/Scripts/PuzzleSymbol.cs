using UnityEngine;

// One glowing symbol the player can touch with E.
public class PuzzleSymbol : Interactable
{
    public int id;
    public string symbolName = "Symbol";
    public Renderer orb;
    public Color glowColor = Color.white;
    public PuzzleManager puzzle;

    private bool lit;

    public override bool CanInteract => puzzle != null && !puzzle.Solved && !lit;

    public override string GetPrompt()
    {
        return "Press E - Touch the " + symbolName + " symbol";
    }

    private void Start()
    {
        SetLit(false);
    }

    public override void Interact()
    {
        if (puzzle == null)
        {
            Debug.LogError("PuzzleSymbol: no PuzzleManager assigned on " + name);
            return;
        }
        puzzle.Press(this);
    }

    public void SetLit(bool value)
    {
        lit = value;
        MaterialGlow.Apply(orb, glowColor, lit ? 3f : 0.1f);
    }
}
