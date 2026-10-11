using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// Menu: Echoes > Setup Crystals and Gate
// Adds the game systems, 5 crystals, a sealed gate and a magic switch. Safe to run again.
public static class MagicSetupBuilder
{
    private const string RootName = "Magic_Setup";

    [MenuItem("Echoes/Setup Crystals and Gate")]
    public static void Build()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("MagicSetupBuilder: no object with the 'Player' tag found.");
            return;
        }

        // Remove old setup
        GameObject old = GameObject.Find(RootName);
        if (old != null) Undo.DestroyObjectImmediate(old);
        GameObject oldSystems = GameObject.Find("GameSystems");
        if (oldSystems != null) Undo.DestroyObjectImmediate(oldSystems);

        // Player interaction
        if (player.GetComponent<PlayerInteraction>() == null)
            Undo.AddComponent<PlayerInteraction>(player);

        // Systems object
        GameObject systems = new GameObject("GameSystems");
        Undo.RegisterCreatedObjectUndo(systems, "Setup");
        systems.AddComponent<MagicSystem>();
        systems.AddComponent<HUDController>();

        GameObject root = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(root, "Setup");

        Material crystalMat = GetGlowMat("Crystal_Glow", new Color(0.3f, 0.8f, 1f), new Color(0.3f, 0.8f, 1f));
        Material gateMat = GetGlowMat("Gate_Sealed", new Color(0.45f, 0.2f, 0.6f), new Color(0.5f, 0.1f, 0.8f));
        Material pedestalMat = GetGlowMat("Pedestal_Stone", new Color(0.3f, 0.3f, 0.34f), Color.black);
        Material orbMat = GetGlowMat("Switch_Orb", new Color(1f, 0.3f, 0.3f), new Color(1f, 0.2f, 0.2f));

        // Crystals
        Vector3[] spots =
        {
            new Vector3(-10, 1.2f, -8), new Vector3(10, 1.2f, -10), new Vector3(-12, 1.2f, 8),
            new Vector3(12, 1.2f, 6),   new Vector3(-8, 1.2f, 14)
        };
        for (int i = 0; i < spots.Length; i++)
        {
            GameObject crystal = new GameObject("Crystal_" + (i + 1));
            crystal.transform.SetParent(root.transform, false);
            crystal.transform.position = spots[i];
            SphereCollider col = crystal.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 1.1f;
            crystal.AddComponent<CollectibleCrystal>();

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = "Visual";
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            visual.transform.SetParent(crystal.transform, false);
            visual.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
            visual.transform.localRotation = Quaternion.Euler(0, 0, 20);
            visual.GetComponent<Renderer>().sharedMaterial = crystalMat;
        }

        // Sealed gate in the south wall gap
        GameObject gate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gate.name = "SealedGate";
        gate.transform.SetParent(root.transform, false);
        gate.transform.position = new Vector3(0, 3, -20);
        gate.transform.localScale = new Vector3(6, 6, 1.2f);
        gate.GetComponent<Renderer>().sharedMaterial = gateMat;
        DoorController door = gate.AddComponent<DoorController>();

        // Switch: root + pedestal + glowing orb
        GameObject sw = new GameObject("MagicSwitch");
        sw.transform.SetParent(root.transform, false);
        sw.transform.position = new Vector3(0, 0, -15);

        GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pedestal.name = "Pedestal";
        pedestal.transform.SetParent(sw.transform, false);
        pedestal.transform.localPosition = new Vector3(0, 0.75f, 0);
        pedestal.transform.localScale = new Vector3(0.8f, 0.75f, 0.8f);
        pedestal.GetComponent<Renderer>().sharedMaterial = pedestalMat;

        GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        orb.name = "Orb";
        Object.DestroyImmediate(orb.GetComponent<Collider>());
        orb.transform.SetParent(sw.transform, false);
        orb.transform.localPosition = new Vector3(0, 1.9f, 0);
        orb.transform.localScale = Vector3.one * 0.6f;
        orb.GetComponent<Renderer>().sharedMaterial = orbMat;

        MagicSwitch magicSwitch = sw.AddComponent<MagicSwitch>();
        magicSwitch.door = door;
        magicSwitch.indicator = orb.GetComponent<Renderer>();
        magicSwitch.energyCost = 30f;

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Crystals, gate and switch created. Press Ctrl+S to save the scene.");
    }

    private static Material GetGlowMat(string name, Color baseColor, Color glow)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");

        string path = "Assets/Materials/" + name + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m != null) return m;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        m = new Material(shader);
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", baseColor);
        if (m.HasProperty("_Color")) m.SetColor("_Color", baseColor);
        if (glow != Color.black && m.HasProperty("_EmissionColor"))
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", glow * 2f);
        }
        AssetDatabase.CreateAsset(m, path);
        return m;
    }
}
