using System.Collections.Generic;
using UnityEngine;

// A safe point. The demon cannot enter it. Resting here heals the player and saves a checkpoint.
// Needs a trigger SphereCollider (the setup tool adds it).
public class SafeZone : MonoBehaviour
{
    private static readonly List<SafeZone> zones = new List<SafeZone>();

    public float radius = 4f;
    public Transform respawnPoint;
    public float healPerSecond = 15f;

    private bool visited;

    private void OnEnable()
    {
        if (!zones.Contains(this)) zones.Add(this);
    }

    private void OnDisable()
    {
        zones.Remove(this);
    }

    // True if the point is inside any safe zone (ignores height).
    public static bool IsSafe(Vector3 point)
    {
        foreach (SafeZone z in zones)
        {
            if (z == null) continue;
            if (FlatDistance(point, z.transform.position) <= z.radius) return true;
        }
        return false;
    }

    // If the point is inside a safe zone, gives the nearest position just outside it.
    public static bool TryPushOut(Vector3 point, out Vector3 pushed)
    {
        foreach (SafeZone z in zones)
        {
            if (z == null) continue;
            Vector3 center = z.transform.position;
            if (FlatDistance(point, center) >= z.radius) continue;

            Vector3 dir = point - center;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.001f) dir = Vector3.forward;
            dir.Normalize();

            pushed = center + dir * (z.radius + 1f);
            pushed.y = point.y;
            return true;
        }
        pushed = point;
        return false;
    }

    private static float FlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc != null && respawnPoint != null) pc.SetRespawnPoint(respawnPoint);

        if (!visited && MagicSystem.Instance != null)
        {
            visited = true;
            MagicSystem.Instance.PostMessage("Safe point! The demon cannot enter here. Checkpoint saved.", 4f);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (PlayerHealth.Instance != null)
            PlayerHealth.Instance.Heal(healPerSecond * Time.deltaTime);
    }
}
