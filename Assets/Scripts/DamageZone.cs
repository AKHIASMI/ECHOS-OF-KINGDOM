using UnityEngine;

// Put on an object with a trigger collider. Hurts the player while they stand inside.
public class DamageZone : MonoBehaviour
{
    [SerializeField] private float damagePerSecond = 10f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (MagicSystem.Instance != null)
            MagicSystem.Instance.PostMessage("Cursed ground! Step away!");
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (PlayerHealth.Instance != null)
            PlayerHealth.Instance.TakeDamage(damagePerSecond * Time.deltaTime);
    }
}
