using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Menu: Echoes > Add Sword Combat and Demon Health
// Adds PlayerSwordCombat to the Player and DemonHealth to every demon in the scene.
// Safe to run again. Re-run it after "Build World and Demon Upgrade", which rebuilds the demon.
// Keep this file inside an Editor folder (next to WorldUpgradeBuilder.cs).
public static class CombatUpgradeBuilder
{
    [MenuItem("Echoes/Add Sword Combat and Demon Health")]
    public static void Build()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("CombatUpgradeBuilder: no object with the 'Player' tag found.");
            return;
        }

        if (player.GetComponent<PlayerSwordCombat>() == null)
            Undo.AddComponent<PlayerSwordCombat>(player);

        int demons = 0;
        foreach (DemonAI ai in Object.FindObjectsByType<DemonAI>())
        {
            if (ai.GetComponent<DemonHealth>() == null)
                Undo.AddComponent<DemonHealth>(ai.gameObject);
            demons++;
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Combat added: player sword + " + demons + " demon(s) with health. Press F in play mode to swing. Ctrl+S to save.");
    }
}
