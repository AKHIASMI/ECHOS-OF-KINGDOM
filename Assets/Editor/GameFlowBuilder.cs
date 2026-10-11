using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

// Menu: Echoes > Setup Menus and Game Flow
// Open your level scene first. This tool:
//  - adds GameManager, pause/game-over UI, player health and two cursed-ground hazards
//  - saves the scene as Assets/Scenes/Level1_ForgottenCourtyard.unity
//  - creates the MainMenu scene
//  - adds the scenes to the build settings
public static class GameFlowBuilder
{
    private const string Level1Path = "Assets/Scenes/Level1_ForgottenCourtyard.unity";
    private const string MenuPath = "Assets/Scenes/MainMenu.unity";

    [MenuItem("Echoes/Setup Menus and Game Flow")]
    public static void Build()
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        if (scene.name == "MainMenu")
        {
            Debug.LogError("GameFlowBuilder: open your LEVEL scene (e.g. SampleScene) first, not the MainMenu scene.");
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("GameFlowBuilder: no object with the 'Player' tag found in this scene.");
            return;
        }

        // Player health
        if (player.GetComponent<PlayerHealth>() == null)
            Undo.AddComponent<PlayerHealth>(player);

        // Remove old versions
        DestroyIfExists("GameFlow");
        DestroyIfExists("Hazards_Setup");

        // GameManager + pause/game-over UI
        GameObject flow = new GameObject("GameFlow");
        Undo.RegisterCreatedObjectUndo(flow, "Game Flow");
        flow.AddComponent<GameManager>();
        flow.AddComponent<PauseMenuUI>();

        // Hazards
        GameObject hazards = new GameObject("Hazards_Setup");
        Undo.RegisterCreatedObjectUndo(hazards, "Hazards");
        Material hazardMat = GetGlowMat("Hazard_Cursed", new Color(0.5f, 0.05f, 0.05f), new Color(0.9f, 0.1f, 0.1f));
        CreateHazard(hazards.transform, "CursedGround_1", new Vector3(-9f, 0f, -3f), hazardMat);
        CreateHazard(hazards.transform, "CursedGround_2", new Vector3(8f, 0f, -9f), hazardMat);

        EditorSceneManager.MarkSceneDirty(scene);

        // Save the level scene under its final name
        string levelPath = scene.path;
        if (scene.name == "SampleScene" || string.IsNullOrEmpty(scene.path))
        {
            if (!EditorSceneManager.SaveScene(scene, Level1Path))
            {
                Debug.LogError("GameFlowBuilder: could not save the scene as " + Level1Path);
                return;
            }
            levelPath = Level1Path;
        }
        else
        {
            EditorSceneManager.SaveScene(scene);
        }

        // Create the main menu scene if it doesn't exist
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(MenuPath) == null)
        {
            Scene menu = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            GameObject go = new GameObject("MainMenu");
            go.AddComponent<MainMenuUI>();

            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.05f, 0.04f, 0.1f);
            }
            EditorSceneManager.SaveScene(menu, MenuPath);
            EditorSceneManager.OpenScene(levelPath);
        }

        // Build settings: menu first, then every level scene that exists
        List<EditorBuildSettingsScene> list = new List<EditorBuildSettingsScene>();
        List<string> paths = new List<string> { MenuPath };
        foreach (string n in LevelManager.SceneNames) paths.Add("Assets/Scenes/" + n + ".unity");
        if (!paths.Contains(levelPath)) paths.Add(levelPath);

        foreach (string p in paths)
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(p) != null)
                list.Add(new EditorBuildSettingsScene(p, true));
        EditorBuildSettings.scenes = list.ToArray();

        Debug.Log("Game flow ready. Open Assets/Scenes/MainMenu and press Play to test the whole game.");
    }

    private static void DestroyIfExists(string name)
    {
        GameObject old = GameObject.Find(name);
        if (old != null) Undo.DestroyObjectImmediate(old);
    }

    private static void CreateHazard(Transform parent, string name, Vector3 pos, Material mat)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = pos;

        BoxCollider trigger = root.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = new Vector3(0, 1, 0);
        trigger.size = new Vector3(4, 2, 4);
        root.AddComponent<DamageZone>();

        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Visual";
        Object.DestroyImmediate(visual.GetComponent<Collider>());
        visual.transform.SetParent(root.transform, false);
        visual.transform.localPosition = new Vector3(0, 0.03f, 0);
        visual.transform.localScale = new Vector3(4, 0.06f, 4);
        visual.GetComponent<Renderer>().sharedMaterial = mat;
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
        if (m.HasProperty("_EmissionColor"))
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", glow * 1.5f);
        }
        AssetDatabase.CreateAsset(m, path);
        return m;
    }
}
