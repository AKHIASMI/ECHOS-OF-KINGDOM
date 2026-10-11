using UnityEngine;

// Put this on the Player. Press F (or left mouse button) to cast an Arcane Bolt at the
// nearest demon in front of the camera. Costs magic energy, so collecting crystals matters.
public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float energyCost = 8f;
    [SerializeField] private float damage = 30f;
    [SerializeField] private float range = 14f;
    [SerializeField] private float cooldown = 0.6f;
    [SerializeField] private KeyCode castKey = KeyCode.F;

    private float nextCast;

    private void Update()
    {
        if (Time.timeScale == 0f) return;
        if (PlayerHealth.Instance != null && PlayerHealth.Instance.IsDead) return;
        if (!Input.GetKeyDown(castKey) && !Input.GetMouseButtonDown(0)) return;
        if (Time.time < nextCast) return;

        MagicSystem magic = MagicSystem.Instance;

        DemonHealth target = FindTarget();
        if (target == null)
        {
            if (magic != null) magic.PostMessage("No demon in front of you.", 1f);
            return;
        }

        if (magic == null || !magic.TrySpend(energyCost))
        {
            if (magic != null) magic.PostMessage("Not enough magic energy! Collect crystals.", 1.5f);
            return;
        }

        nextCast = Time.time + cooldown;
        target.TakeDamage(damage);
        SpawnHitFlash(target.transform.position + Vector3.up * 1.2f);
    }

    // Nearest living demon within range and roughly in front of where the camera is looking.
    private DemonHealth FindTarget()
    {
        Vector3 aim = transform.forward;
        if (Camera.main != null) aim = Camera.main.transform.forward;
        aim.y = 0f;
        if (aim.sqrMagnitude < 0.001f) aim = transform.forward;
        aim.Normalize();

        DemonHealth best = null;
        float bestDist = range;

        foreach (DemonHealth d in DemonHealth.All)
        {
            if (d == null || d.IsDead) continue;

            Vector3 to = d.transform.position - transform.position;
            to.y = 0f;
            float dist = to.magnitude;
            if (dist > bestDist) continue;
            if (dist > 0.5f && Vector3.Dot(aim, to / dist) < 0.5f) continue;

            best = d;
            bestDist = dist;
        }
        return best;
    }

    private void SpawnHitFlash(Vector3 position)
    {
        GameObject fx = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Collider col = fx.GetComponent<Collider>();
        if (col != null) Destroy(col);

        fx.transform.position = position;
        fx.transform.localScale = Vector3.one * 0.9f;

        Renderer rend = fx.GetComponent<Renderer>();
        if (rend != null) rend.material.color = new Color(0.4f, 0.7f, 1f);

        Destroy(fx, 0.2f);
    }
}
