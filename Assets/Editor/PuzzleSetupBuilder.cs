using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// Menu: Echoes > Setup Puzzle and Portal
// Run "Echoes > Setup Crystals and Gate" first. Safe to run again.
public static class PuzzleSetupBuilder
{
    private const string RootName = "Puzzle_Setup";

    [MenuItem("Echoes/Setup Puzzle and Portal")]
    public static void Build()
    {
        GameObject systems = GameObject.Find("GameSystems");
        if (systems == null)
        {
            Debug.LogError("PuzzleSetupBuilder: run 'Echoes > Setup Crystals and Gate' first.");
            return;
        }

        GameObject old = GameObject.Find(RootName);
        if (old != null) Undo.DestroyObjectImmediate(old);

        if (systems.GetComponent<QuestManager>() == null)
            Undo.AddComponent<QuestManager>(systems);

        GameObject root = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(root, "Puzzle Setup");

        Material stoneMat = GetMat("Pedestal_Stone", new Color(0.3f, 0.3f, 0.34f));
        Material orbMat = GetMat("Symbol_Orb", new Color(0.8f, 0.8f, 0.8f));
        Material tabletMat = GetMat("Tablet_Stone", new Color(0.22f, 0.22f, 0.26f));
        Material portalMat = GetMat("Portal_Disc", new Color(0.25f, 0.25f, 0.3f));

        // --- Symbols (east side of the courtyard, in front of the keep) ---
        string[] names = { "Moon", "Star", "Sun" };
        Color[] colors =
        {
            new Color(0.5f, 0.65f, 1f),   // Moon
            new Color(1f, 0.5f, 0.9f),    // Star
            new Color(1f, 0.75f, 0.2f)    // Sun
        };
        float[] zs = { 11f, 14.5f, 18f };
        PuzzleSymbol[] symbols = new PuzzleSymbol[3];

        for (int i = 0; i < 3; i++)
        {
            GameObject sym = new GameObject("Symbol_" + names[i]);
            sym.transform.SetParent(root.transform, false);
            sym.transform.position = new Vector3(14f, 0f, zs[i]);

            GameObject ped = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ped.name = "Pedestal";
            ped.transform.SetParent(sym.transform, false);
            ped.transform.localPosition = new Vector3(0, 0.75f, 0);
            ped.transform.localScale = new Vector3(0.8f, 0.75f, 0.8f);
            ped.GetComponent<Renderer>().sharedMaterial = stoneMat;

            GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.name = "Orb";
            Object.DestroyImmediate(orb.GetComponent<Collider>());
            orb.transform.SetParent(sym.transform, false);
            orb.transform.localPosition = new Vector3(0, 1.9f, 0);
            orb.transform.localScale = Vector3.one * 0.6f;
            orb.GetComponent<Renderer>().sharedMaterial = orbMat;

            PuzzleSymbol ps = sym.AddComponent<PuzzleSymbol>();
            ps.id = i;
            ps.symbolName = names[i];
            ps.orb = orb.GetComponent<Renderer>();
            ps.glowColor = colors[i];
            symbols[i] = ps;
        }

        // --- Portal in front of the keep ---
        GameObject portalRoot = new GameObject("MagicPortal");
        portalRoot.transform.SetParent(root.transform, false);
        portalRoot.transform.position = new Vector3(0f, 0f, 8f);
        BoxCollider trigger = portalRoot.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = new Vector3(0, 2, 0);
        trigger.size = new Vector3(3, 4, 2);

        GameObject disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        disc.name = "PortalDisc";
        Object.DestroyImmediate(disc.GetComponent<Collider>());
        disc.transform.SetParent(portalRoot.transform, false);
        disc.transform.localPosition = new Vector3(0, 2, 0);
        disc.transform.localRotation = Quaternion.Euler(90, 0, 0);
        disc.transform.localScale = new Vector3(3, 0.1f, 3);
        disc.GetComponent<Renderer>().sharedMaterial = portalMat;

        MagicPortal portal = portalRoot.AddComponent<MagicPortal>();
        portal.visual = disc.GetComponent<Renderer>();

        // --- Puzzle manager: correct order is Sun, Moon, Star (ids 2, 0, 1) ---
        GameObject pmObj = new GameObject("PuzzleManager");
        pmObj.transform.SetParent(root.transform, false);
        PuzzleManager puzzle = pmObj.AddComponent<PuzzleManager>();
        puzzle.symbols = symbols;
        puzzle.correctOrder = new int[] { 2, 0, 1 };
        puzzle.portal = portal;
        foreach (PuzzleSymbol ps in symbols) ps.puzzle = puzzle;

        // --- Ancient tablet (the hint) ---
        GameObject tablet = new GameObject("AncientTablet");
        tablet.transform.SetParent(root.transform, false);
        tablet.transform.position = new Vector3(9f, 0f, 9.5f);

        GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        slab.name = "Slab";
        slab.transform.SetParent(tablet.transform, false);
        slab.transform.localPosition = new Vector3(0, 0.9f, 0);
        slab.transform.localScale = new Vector3(1.2f, 1.8f, 0.3f);
        slab.GetComponent<Renderer>().sharedMaterial = tabletMat;

        AncientTablet at = tablet.AddComponent<AncientTablet>();
        at.puzzle = puzzle;

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Puzzle and portal created. Press Ctrl+S to save the scene.");
    }

    private static Material GetMat(string name, Color color)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");

        string path = "Assets/Materials/" + name + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m != null) return m;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        m = new Material(shader);
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
        if (m.HasProperty("_Color")) m.SetColor("_Color", color);
        AssetDatabase.CreateAsset(m, path);
        return m;
    }
}
