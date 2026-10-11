using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using UnityEditor.SceneManagement;

// Menu: Echoes > Build World and Demon Upgrade
// Adds: knight avatar for the player, detailed pillars, road, lamp posts and trees outside the castle,
// rocks and distant mountains, safe zones, a chasing demon and the navigation mesh baker.
// Safe to run again.
public static class WorldUpgradeBuilder
{
    private const string RootName = "World_Upgrade";

    [MenuItem("Echoes/Build World and Demon Upgrade")]
    public static void Build()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("WorldUpgradeBuilder: no object with the 'Player' tag found.");
            return;
        }

        // ---------- clean up older versions ----------
        DestroyByName(RootName);
        DestroyByName("NavMesh_Baker");
        Transform oldAvatar = player.transform.Find("Avatar");
        if (oldAvatar != null) Undo.DestroyObjectImmediate(oldAvatar.gameObject);

        GameObject castle = GameObject.Find("Castle_Environment");
        if (castle != null)
        {
            List<GameObject> oldPillars = new List<GameObject>();
            foreach (Transform child in castle.transform)
                if (child.name == "Pillar") oldPillars.Add(child.gameObject);
            foreach (GameObject g in oldPillars) Undo.DestroyObjectImmediate(g);
        }

        // ---------- bigger ground so there is room outside the castle ----------
        GameObject ground = GameObject.Find("ground");
        if (ground == null) ground = GameObject.Find("Ground");
        if (ground != null) ground.transform.localScale = new Vector3(12f, 1f, 12f);

        GameObject root = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(root, "World Upgrade");
        Transform rt = root.transform;

        BuildAvatar(player);
        BuildPillars(rt);
        BuildRoadAndLamps(rt);
        BuildTrees(rt);
        BuildRocksAndMountains(rt);
        BuildSafeZones(rt);
        BuildDemon(rt);

        GameObject baker = new GameObject("NavMesh_Baker");
        Undo.RegisterCreatedObjectUndo(baker, "NavMesh Baker");
        baker.AddComponent<NavMeshBaker>();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("World and demon upgrade built. Press Ctrl+S to save, then Play.");
    }

    // ============================================================ PLAYER AVATAR
    private static void BuildAvatar(GameObject player)
    {
        MeshRenderer capsule = player.GetComponent<MeshRenderer>();
        if (capsule != null) capsule.enabled = false;

        Material armor = M("Armor", new Color(0.35f, 0.40f, 0.50f));
        Material cloak = M("Cloak", new Color(0.60f, 0.10f, 0.15f));
        Material skin = M("Skin", new Color(0.85f, 0.65f, 0.50f));
        Material leather = M("Leather", new Color(0.20f, 0.14f, 0.10f));
        Material steel = M("Steel", new Color(0.75f, 0.77f, 0.80f));
        Material glow = M("AmuletGlow", new Color(0.3f, 0.8f, 1f), 2f);

        GameObject av = new GameObject("Avatar");
        av.transform.SetParent(player.transform, false);

        Transform body = new GameObject("Body").transform;
        body.SetParent(av.transform, false);

        P(PrimitiveType.Cube, "Torso", body, new Vector3(0, 0.25f, 0), new Vector3(0.6f, 0.8f, 0.35f), armor);
        P(PrimitiveType.Cube, "Belt", body, new Vector3(0, -0.12f, 0), new Vector3(0.62f, 0.12f, 0.37f), leather);
        P(PrimitiveType.Sphere, "Head", body, new Vector3(0, 0.85f, 0), new Vector3(0.36f, 0.4f, 0.36f), skin);
        P(PrimitiveType.Sphere, "Helmet", body, new Vector3(0, 0.93f, -0.02f), new Vector3(0.4f, 0.32f, 0.4f), armor);
        P(PrimitiveType.Cube, "Cloak", body, new Vector3(0, 0.1f, -0.24f), new Vector3(0.58f, 1.0f, 0.07f), cloak);
        P(PrimitiveType.Sphere, "Amulet", body, new Vector3(0, 0.4f, 0.19f), Vector3.one * 0.12f, glow);
        GameObject sword = P(PrimitiveType.Cube, "Sword", body, new Vector3(0.2f, 0.3f, -0.3f), new Vector3(0.06f, 1.1f, 0.03f), steel);
        sword.transform.localRotation = Quaternion.Euler(0, 0, 20);

        Transform legL = Pivot("LegL", av.transform, new Vector3(-0.17f, -0.15f, 0f));
        Transform legR = Pivot("LegR", av.transform, new Vector3(0.17f, -0.15f, 0f));
        foreach (Transform leg in new[] { legL, legR })
        {
            P(PrimitiveType.Cube, "Leg", leg, new Vector3(0, -0.42f, 0), new Vector3(0.24f, 0.85f, 0.26f), armor);
            P(PrimitiveType.Cube, "Boot", leg, new Vector3(0, -0.9f, 0.05f), new Vector3(0.26f, 0.2f, 0.34f), leather);
        }

        Transform armL = Pivot("ArmL", av.transform, new Vector3(-0.42f, 0.55f, 0f));
        Transform armR = Pivot("ArmR", av.transform, new Vector3(0.42f, 0.55f, 0f));
        foreach (Transform arm in new[] { armL, armR })
        {
            P(PrimitiveType.Cube, "Arm", arm, new Vector3(0, -0.3f, 0), new Vector3(0.18f, 0.7f, 0.2f), armor);
            P(PrimitiveType.Cube, "Hand", arm, new Vector3(0, -0.68f, 0), new Vector3(0.16f, 0.16f, 0.18f), skin);
        }

        PlayerAvatarAnimator anim = av.AddComponent<PlayerAvatarAnimator>();
        anim.legL = legL;
        anim.legR = legR;
        anim.armL = armL;
        anim.armR = armR;
        anim.body = body;
    }

    // ============================================================ PILLARS
    private static void BuildPillars(Transform parent)
    {
        Material stone = M("Stone", new Color(0.50f, 0.48f, 0.45f));
        Material dark = M("StoneDark", new Color(0.36f, 0.35f, 0.38f));

        float[] zs = { -12f, -4f, 4f };
        int index = 0;
        foreach (float z in zs)
        {
            foreach (float x in new[] { -5f, 5f })
            {
                bool broken = index % 2 == 1;
                index++;

                GameObject pillar = new GameObject("Pillar");
                pillar.transform.SetParent(parent, false);
                pillar.transform.position = new Vector3(x, 0, z);
                Transform t = pillar.transform;

                P(PrimitiveType.Cube, "Base", t, new Vector3(0, 0.25f, 0), new Vector3(1.7f, 0.5f, 1.7f), dark, true);

                float shaftH = broken ? 3.0f : 4.8f;
                P(PrimitiveType.Cylinder, "Shaft", t, new Vector3(0, 0.5f + shaftH / 2f, 0), new Vector3(1.2f, shaftH / 2f, 1.2f), stone, true);
                P(PrimitiveType.Cylinder, "RingLow", t, new Vector3(0, 0.8f, 0), new Vector3(1.5f, 0.06f, 1.5f), dark);

                if (!broken)
                {
                    P(PrimitiveType.Cylinder, "RingHigh", t, new Vector3(0, 5.0f, 0), new Vector3(1.5f, 0.06f, 1.5f), dark);
                    P(PrimitiveType.Cube, "Capital", t, new Vector3(0, 5.55f, 0), new Vector3(1.5f, 0.5f, 1.5f), dark);
                }
                else
                {
                    GameObject r1 = P(PrimitiveType.Cube, "Rubble", t, new Vector3(1.1f, 0.25f, 0.4f), new Vector3(0.8f, 0.5f, 0.7f), stone, true);
                    r1.transform.localRotation = Quaternion.Euler(0, 25, 0);
                    GameObject r2 = P(PrimitiveType.Cube, "Rubble", t, new Vector3(-1.0f, 0.2f, -0.6f), new Vector3(0.5f, 0.4f, 0.5f), stone, true);
                    r2.transform.localRotation = Quaternion.Euler(0, -35, 0);
                }
            }
        }
    }

    // ============================================================ ROAD + LAMP POSTS (outside the castle)
    private static void BuildRoadAndLamps(Transform parent)
    {
        Material dirt = M("Dirt", new Color(0.35f, 0.27f, 0.18f));
        Material iron = M("Iron", new Color(0.12f, 0.12f, 0.14f));
        Material lantern = M("Lantern", new Color(1f, 0.7f, 0.3f), 3f);

        P(PrimitiveType.Cube, "Road", parent, new Vector3(0, 0.03f, -40f), new Vector3(6f, 0.06f, 40f), dirt);

        // Posts every 6 units along both sides of the road, starting at the castle gate
        for (int i = 0; i < 6; i++)
        {
            float z = -24f - i * 6f;
            LampPost(parent, new Vector3(-4.2f, 0, z), iron, lantern);
            LampPost(parent, new Vector3(4.2f, 0, z), iron, lantern);
        }
    }

    private static void LampPost(Transform parent, Vector3 pos, Material iron, Material lantern)
    {
        GameObject post = new GameObject("LampPost");
        post.transform.SetParent(parent, false);
        post.transform.position = pos;
        Transform t = post.transform;

        P(PrimitiveType.Cylinder, "Base", t, new Vector3(0, 0.15f, 0), new Vector3(0.45f, 0.15f, 0.45f), iron);
        P(PrimitiveType.Cylinder, "Pole", t, new Vector3(0, 2.15f, 0), new Vector3(0.14f, 2.0f, 0.14f), iron, true);
        P(PrimitiveType.Cube, "Lantern", t, new Vector3(0, 4.45f, 0), new Vector3(0.5f, 0.6f, 0.5f), lantern);
        P(PrimitiveType.Cube, "Cap", t, new Vector3(0, 4.85f, 0), new Vector3(0.7f, 0.12f, 0.7f), iron);

        GameObject lightObj = new GameObject("Light");
        lightObj.transform.SetParent(t, false);
        lightObj.transform.localPosition = new Vector3(0, 4.45f, 0);
        Light l = lightObj.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(1f, 0.7f, 0.4f);
        l.range = 10f;
        l.intensity = 4f;
        l.shadows = LightShadows.None;
        lightObj.AddComponent<LampFlicker>();
    }

    // ============================================================ TREES (outside the castle)
    private static void BuildTrees(Transform parent)
    {
        Material bark = M("Bark", new Color(0.25f, 0.17f, 0.10f));
        Material leafA = M("LeafA", new Color(0.12f, 0.30f, 0.14f));
        Material leafB = M("LeafB", new Color(0.18f, 0.36f, 0.16f));
        Material leafC = M("LeafC", new Color(0.30f, 0.30f, 0.12f));

        System.Random rng = new System.Random(11);
        int placed = 0;
        int guard = 0;
        while (placed < 70 && guard < 1000)
        {
            guard++;
            double angle = rng.NextDouble() * System.Math.PI * 2.0;
            double radius = 34.0 + rng.NextDouble() * 22.0;
            float x = (float)(System.Math.Cos(angle) * radius);
            float z = (float)(System.Math.Sin(angle) * radius);

            if (Mathf.Abs(x) < 8f && z < -18f) continue; // keep the road clear

            float f = 0.8f + (float)rng.NextDouble() * 0.7f;
            Material leaf = rng.NextDouble() < 0.3 ? leafC : (rng.NextDouble() < 0.5 ? leafA : leafB);

            GameObject tree = new GameObject("Tree");
            tree.transform.SetParent(parent, false);
            tree.transform.position = new Vector3(x, 0, z);
            tree.transform.localScale = Vector3.one * f;
            tree.transform.rotation = Quaternion.Euler(0, (float)(rng.NextDouble() * 360.0), 0);
            Transform t = tree.transform;

            P(PrimitiveType.Cylinder, "Trunk", t, new Vector3(0, 1.8f, 0), new Vector3(0.5f, 1.8f, 0.5f), bark, true);
            P(PrimitiveType.Sphere, "Leaves1", t, new Vector3(0, 4.3f, 0), new Vector3(3.4f, 2.6f, 3.4f), leaf);
            P(PrimitiveType.Sphere, "Leaves2", t, new Vector3(0, 5.7f, 0), new Vector3(2.6f, 2.2f, 2.6f), leaf);
            P(PrimitiveType.Sphere, "Leaves3", t, new Vector3(0, 6.9f, 0), new Vector3(1.7f, 1.6f, 1.7f), leaf);
            placed++;
        }
    }

    // ============================================================ ROCKS + MOUNTAINS
    private static void BuildRocksAndMountains(Transform parent)
    {
        Material rock = M("Rock", new Color(0.40f, 0.40f, 0.42f));
        Material mountain = M("Mountain", new Color(0.35f, 0.38f, 0.45f));

        System.Random rng = new System.Random(23);
        for (int i = 0; i < 30; i++)
        {
            double angle = rng.NextDouble() * System.Math.PI * 2.0;
            double radius = 27.0 + rng.NextDouble() * 31.0;
            float x = (float)(System.Math.Cos(angle) * radius);
            float z = (float)(System.Math.Sin(angle) * radius);
            if (Mathf.Abs(x) < 8f && z < -18f) continue;

            float s = 0.8f + (float)rng.NextDouble() * 1.8f;
            GameObject r = P(PrimitiveType.Sphere, "Rock", parent, new Vector3(x, s * 0.3f, z),
                             new Vector3(s * 1.3f, s * 0.8f, s), rock, true);
            r.transform.rotation = Quaternion.Euler(0, (float)(rng.NextDouble() * 360.0), 0);
        }

        // Far-away mountains that fade into the fog
        for (int i = 0; i < 12; i++)
        {
            float angle = i * 30f * Mathf.Deg2Rad;
            float radius = 85f + (i % 3) * 8f;
            float w = 40f + (i % 4) * 8f;
            float h = 24f + (i % 5) * 6f;
            P(PrimitiveType.Sphere, "Mountain", parent,
              new Vector3(Mathf.Cos(angle) * radius, -4f, Mathf.Sin(angle) * radius),
              new Vector3(w, h, w), mountain);
        }
    }

    // ============================================================ SAFE ZONES
    private static void BuildSafeZones(Transform parent)
    {
        Material ring = M("SafeRing", new Color(0.2f, 0.6f, 1f), 0.6f);
        Material shrine = M("Stone", new Color(0.50f, 0.48f, 0.45f));
        Material orb = M("SafeOrb", new Color(0.5f, 0.85f, 1f), 3f);

        BuildSafeZone(parent, "SafeZone_Gate", new Vector3(0, 0, -13.5f), 4f, ring, shrine, orb);
        BuildSafeZone(parent, "SafeZone_Puzzle", new Vector3(11f, 0, 14f), 5.5f, ring, shrine, orb);
        BuildSafeZone(parent, "SafeZone_Portal", new Vector3(0, 0, 6.5f), 4f, ring, shrine, orb);
    }

    private static void BuildSafeZone(Transform parent, string name, Vector3 pos, float radius,
                                      Material ring, Material shrine, Material orb)
    {
        GameObject z = new GameObject(name);
        z.transform.SetParent(parent, false);
        z.transform.position = pos;

        SphereCollider trigger = z.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = radius;
        trigger.center = new Vector3(0, 1f, 0);

        SafeZone sz = z.AddComponent<SafeZone>();
        sz.radius = radius;

        GameObject respawn = new GameObject("Respawn");
        respawn.transform.SetParent(z.transform, false);
        respawn.transform.localPosition = new Vector3(0, 1f, 0);
        sz.respawnPoint = respawn.transform;

        P(PrimitiveType.Cylinder, "Ring", z.transform, new Vector3(0, 0.04f, 0), new Vector3(radius * 2f, 0.03f, radius * 2f), ring);
        P(PrimitiveType.Cylinder, "Shrine", z.transform, new Vector3(0, 0.8f, 0), new Vector3(0.5f, 0.8f, 0.5f), shrine);
        P(PrimitiveType.Sphere, "Orb", z.transform, new Vector3(0, 1.9f, 0), Vector3.one * 0.7f, orb);

        GameObject lightObj = new GameObject("Light");
        lightObj.transform.SetParent(z.transform, false);
        lightObj.transform.localPosition = new Vector3(0, 2f, 0);
        Light l = lightObj.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(0.5f, 0.8f, 1f);
        l.range = radius * 1.6f;
        l.intensity = 3f;
        l.shadows = LightShadows.None;

        // Cuts the safe area out of the demon's walkable area
        NavMeshObstacle obstacle = z.AddComponent<NavMeshObstacle>();
        obstacle.shape = NavMeshObstacleShape.Capsule;
        obstacle.carving = true;
        obstacle.radius = radius;
        obstacle.height = 3f;
        obstacle.center = new Vector3(0, 1.5f, 0);
    }

    // ============================================================ DEMON
    private static void BuildDemon(Transform parent)
    {
        Material skin = M("DemonSkin", new Color(0.35f, 0.05f, 0.05f));
        Material horn = M("DemonHorn", new Color(0.85f, 0.80f, 0.70f));
        Material eye = M("DemonEye", new Color(1f, 0.1f, 0.05f), 4f);

        GameObject demon = new GameObject("Demon");
        demon.transform.SetParent(parent, false);
        demon.transform.position = new Vector3(-12f, 0f, -6f);

        Transform v = new GameObject("Visual").transform;
        v.SetParent(demon.transform, false);

        P(PrimitiveType.Capsule, "Body", v, new Vector3(0, 1.2f, 0), new Vector3(0.9f, 0.85f, 0.7f), skin);
        P(PrimitiveType.Cube, "LegL", v, new Vector3(-0.25f, 0.35f, 0), new Vector3(0.3f, 0.7f, 0.35f), skin);
        P(PrimitiveType.Cube, "LegR", v, new Vector3(0.25f, 0.35f, 0), new Vector3(0.3f, 0.7f, 0.35f), skin);
        P(PrimitiveType.Cube, "ArmL", v, new Vector3(-0.65f, 1.35f, 0.1f), new Vector3(0.2f, 0.9f, 0.2f), skin);
        P(PrimitiveType.Cube, "ArmR", v, new Vector3(0.65f, 1.35f, 0.1f), new Vector3(0.2f, 0.9f, 0.2f), skin);
        P(PrimitiveType.Sphere, "Head", v, new Vector3(0, 2.25f, 0), Vector3.one * 0.55f, skin);

        GameObject hornL = P(PrimitiveType.Cube, "HornL", v, new Vector3(-0.2f, 2.65f, 0), new Vector3(0.12f, 0.5f, 0.12f), horn);
        hornL.transform.localRotation = Quaternion.Euler(0, 0, 25);
        GameObject hornR = P(PrimitiveType.Cube, "HornR", v, new Vector3(0.2f, 2.65f, 0), new Vector3(0.12f, 0.5f, 0.12f), horn);
        hornR.transform.localRotation = Quaternion.Euler(0, 0, -25);

        P(PrimitiveType.Sphere, "EyeL", v, new Vector3(-0.12f, 2.3f, 0.24f), Vector3.one * 0.1f, eye);
        P(PrimitiveType.Sphere, "EyeR", v, new Vector3(0.12f, 2.3f, 0.24f), Vector3.one * 0.1f, eye);

        GameObject lightObj = new GameObject("Glow");
        lightObj.transform.SetParent(demon.transform, false);
        lightObj.transform.localPosition = new Vector3(0, 1.5f, 0.5f);
        Light l = lightObj.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(1f, 0.2f, 0.1f);
        l.range = 6f;
        l.intensity = 3f;
        l.shadows = LightShadows.None;

        NavMeshAgent agent = demon.AddComponent<NavMeshAgent>();
        agent.radius = 0.6f;
        agent.height = 2.5f;
        agent.speed = 4.5f;
        agent.acceleration = 14f;
        agent.angularSpeed = 360f;
        agent.stoppingDistance = 1.2f;
        agent.enabled = false; // DemonAI turns it on after the navigation mesh is built

        DemonAI ai = demon.AddComponent<DemonAI>();
        ai.visual = v;
    }

    // ============================================================ HELPERS
    private static void DestroyByName(string name)
    {
        GameObject old = GameObject.Find(name);
        if (old != null) Undo.DestroyObjectImmediate(old);
    }

    private static Transform Pivot(string name, Transform parent, Vector3 localPos)
    {
        GameObject g = new GameObject(name);
        g.transform.SetParent(parent, false);
        g.transform.localPosition = localPos;
        return g.transform;
    }

    private static GameObject P(PrimitiveType type, string name, Transform parent, Vector3 localPos,
                                Vector3 scale, Material mat, bool keepCollider = false)
    {
        GameObject g = GameObject.CreatePrimitive(type);
        g.name = name;
        g.transform.SetParent(parent, false);
        g.transform.localPosition = localPos;
        g.transform.localScale = scale;
        g.GetComponent<Renderer>().sharedMaterial = mat;

        if (!keepCollider)
        {
            Collider c = g.GetComponent<Collider>();
            if (c != null) Object.DestroyImmediate(c);
        }
        return g;
    }

    private static Material M(string name, Color color, float glow = 0f)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");

        string path = "Assets/Materials/Upg_" + name + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m != null) return m;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        m = new Material(shader);
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
        if (m.HasProperty("_Color")) m.SetColor("_Color", color);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.15f);
        if (glow > 0f && m.HasProperty("_EmissionColor"))
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", color * glow);
        }
        AssetDatabase.CreateAsset(m, path);
        return m;
    }
}
