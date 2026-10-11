using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// Adds a menu item: Echoes > Build Castle Environment
// It builds a placeholder castle from Unity primitives, then sets fog, skybox and lighting.
// Safe to run again: it deletes the old "Castle_Environment" object and rebuilds it.
public static class CastleBuilder
{
    private const string RootName = "Castle_Environment";

    [MenuItem("Echoes/Build Castle Environment")]
    public static void Build()
    {
        GameObject old = GameObject.Find(RootName);
        if (old != null) Undo.DestroyObjectImmediate(old);

        Material stone = GetMaterial("Castle_Stone", new Color(0.46f, 0.44f, 0.41f));
        Material darkStone = GetMaterial("Castle_DarkStone", new Color(0.30f, 0.30f, 0.34f));
        Material groundMat = GetMaterial("Castle_Ground", new Color(0.27f, 0.30f, 0.24f));

        GameObject root = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(root, "Build Castle");
        Transform t = root.transform;

        // Paint the existing ground (named "ground" or "Ground")
        GameObject ground = GameObject.Find("ground");
        if (ground == null) ground = GameObject.Find("Ground");
        if (ground != null) ground.GetComponent<Renderer>().sharedMaterial = groundMat;
        else Debug.LogWarning("CastleBuilder: no object named 'ground' found, skipped painting the floor.");

        // Courtyard walls (40 x 40). The south wall has a 6 unit gate gap.
        Box("Wall_North", t, new Vector3(0, 3, 20), new Vector3(40, 6, 2), stone);
        Box("Wall_East", t, new Vector3(20, 3, 0), new Vector3(2, 6, 40), stone);
        Box("Wall_West", t, new Vector3(-20, 3, 0), new Vector3(2, 6, 40), stone);
        Box("Wall_South_Left", t, new Vector3(-11.5f, 3, -20), new Vector3(17, 6, 2), stone);
        Box("Wall_South_Right", t, new Vector3(11.5f, 3, -20), new Vector3(17, 6, 2), stone);
        Box("Gate_Lintel", t, new Vector3(0, 5.5f, -20), new Vector3(6, 1, 2), darkStone);
        Box("Gate_PillarLeft", t, new Vector3(-3.5f, 3, -20), new Vector3(1, 6, 2.4f), darkStone);
        Box("Gate_PillarRight", t, new Vector3(3.5f, 3, -20), new Vector3(1, 6, 2.4f), darkStone);

        // Corner towers
        float[] xs = { -20f, 20f };
        float[] zs = { -20f, 20f };
        foreach (float x in xs)
        {
            foreach (float z in zs)
            {
                Cylinder("Tower", t, new Vector3(x, 5, z), new Vector3(6, 5, 6), stone);
                Box("Tower_Top", t, new Vector3(x, 10.5f, z), new Vector3(7, 1, 7), darkStone);
            }
        }

        // Central keep
        Box("Keep", t, new Vector3(0, 5, 15), new Vector3(14, 10, 8), stone);
        Box("Keep_Top", t, new Vector3(0, 10.5f, 15), new Vector3(15, 1, 9), darkStone);

        // Pillars along the path
        float[] pz = { -12f, -4f, 4f };
        foreach (float z in pz)
        {
            Cylinder("Pillar", t, new Vector3(-5, 3, z), new Vector3(1, 3, 1), darkStone);
            Cylinder("Pillar", t, new Vector3(5, 3, z), new Vector3(1, 3, 1), darkStone);
        }

        // Scattered ruins (fixed seed so it looks the same every time)
        Random.State saved = Random.state;
        Random.InitState(7);
        int placed = 0;
        while (placed < 12)
        {
            Vector2 p = new Vector2(Random.Range(-16f, 16f), Random.Range(-16f, 8f));
            if (p.magnitude < 5f) continue; // keep the spawn area clear
            float s = Random.Range(1f, 2.5f);
            GameObject b = Box("Ruin", t, new Vector3(p.x, s * 0.5f, p.y),
                               new Vector3(s, s, s * Random.Range(0.8f, 1.6f)), darkStone);
            b.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 90f), 0);
            placed++;
        }
        Random.state = saved;

        SetupAtmosphere();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Castle environment built. Press Ctrl+S to save the scene.");
    }

    private static void SetupAtmosphere()
    {
        // Fog
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.012f;
        RenderSettings.fogColor = new Color(0.55f, 0.60f, 0.68f);

        // Skybox (built-in procedural skybox works in URP)
        Shader sky = Shader.Find("Skybox/Procedural");
        if (sky != null)
        {
            string path = "Assets/Materials/Castle_Sky.mat";
            Material skyMat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (skyMat == null)
            {
                skyMat = new Material(sky);
                AssetDatabase.CreateAsset(skyMat, path);
            }
            skyMat.SetFloat("_AtmosphereThickness", 1.3f);
            skyMat.SetFloat("_Exposure", 1.0f);
            skyMat.SetColor("_SkyTint", new Color(0.45f, 0.5f, 0.65f));
            EditorUtility.SetDirty(skyMat);
            RenderSettings.skybox = skyMat;
        }
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 0.9f;

        // Directional light: low warm sun with soft shadows
        Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light l in lights)
        {
            if (l.type != LightType.Directional) continue;
            l.transform.rotation = Quaternion.Euler(38f, -35f, 0f);
            l.color = new Color(1f, 0.92f, 0.78f);
            l.intensity = 1.2f;
            l.shadows = LightShadows.Soft;
            RenderSettings.sun = l;
            break;
        }
        DynamicGI.UpdateEnvironment();
    }

    private static GameObject Box(string name, Transform parent, Vector3 pos, Vector3 scale, Material m)
    {
        return Make(PrimitiveType.Cube, name, parent, pos, scale, m, false);
    }

    private static GameObject Cylinder(string name, Transform parent, Vector3 pos, Vector3 scale, Material m)
    {
        return Make(PrimitiveType.Cylinder, name, parent, pos, scale, m, true);
    }

    private static GameObject Make(PrimitiveType type, string name, Transform parent,
                                   Vector3 pos, Vector3 scale, Material m, bool meshCollider)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = m;
        if (meshCollider)
        {
            // Cylinders come with a capsule collider; a mesh collider fits them better
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.AddComponent<MeshCollider>();
        }
        return go;
    }

    private static Material GetMaterial(string name, Color color)
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
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.15f);
        AssetDatabase.CreateAsset(m, path);
        return m;
    }
}
