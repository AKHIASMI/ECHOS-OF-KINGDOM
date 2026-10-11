using UnityEngine;

// The symbol-order puzzle. Symbols must be touched in the order given by correctOrder (symbol ids).
public class PuzzleManager : MonoBehaviour
{
    public PuzzleSymbol[] symbols;
    public int[] correctOrder = { 0, 1, 2 };
    public MagicPortal portal;

    private int progress;
    private int failures;

    public bool Solved { get; private set; }

    public void Press(PuzzleSymbol pressed)
    {
        if (Solved || pressed == null) return;
        if (correctOrder == null || correctOrder.Length == 0)
        {
            Debug.LogError("PuzzleManager: correctOrder is empty.");
            return;
        }

        MagicSystem magic = MagicSystem.Instance;

        if (pressed.id == correctOrder[progress])
        {
            pressed.SetLit(true);
            progress++;

            if (progress >= correctOrder.Length)
            {
                Solved = true;
                if (magic != null) magic.PostMessage("Puzzle solved! The magic portal awakens!", 4f);
                if (QuestManager.Instance != null) QuestManager.Instance.Complete("puzzle");
                if (portal != null) portal.Activate();
            }
            else if (magic != null)
            {
                magic.PostMessage("The " + pressed.symbolName + " symbol glows softly...");
            }
        }
        else
        {
            failures++;
            progress = 0;
            foreach (PuzzleSymbol s in symbols)
                if (s != null) s.SetLit(false);

            if (magic != null)
            {
                if (failures >= 2)
                    magic.PostMessage("Wrong order! Hint: read the ancient tablet near the keep.", 5f);
                else
                    magic.PostMessage("Wrong order! The symbols fade.");
            }
        }
    }

    public string GetHint()
    {
        string text = "The tablet reads: ";
        for (int i = 0; i < correctOrder.Length; i++)
        {
            string n = "?";
            foreach (PuzzleSymbol s in symbols)
                if (s != null && s.id == correctOrder[i]) n = s.symbolName;

            text += (i == 0 ? "First the " : i == correctOrder.Length - 1 ? "and last the " : "then the ") + n;
            text += i < correctOrder.Length - 1 ? ", " : ".";
        }
        return text;
    }
}
