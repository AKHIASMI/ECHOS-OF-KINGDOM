using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

// Builds the walkable area for the demon when the level starts.
// If you get an error about 'Unity.AI.Navigation', install the "AI Navigation" package
// (Window > Package Manager > Unity Registry > AI Navigation > Install).
public class NavMeshBaker : MonoBehaviour
{
    private void Awake()
    {
        NavMeshSurface surface = GetComponent<NavMeshSurface>();
        if (surface == null) surface = gameObject.AddComponent<NavMeshSurface>();

        surface.collectObjects = CollectObjects.All;
        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        surface.BuildNavMesh();
    }
}
