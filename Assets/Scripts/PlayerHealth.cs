using UnityEngine;

// Put this on the Player. Handles damage, slow regeneration and death.
public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float regenDelay = 6f;
    [SerializeField] private float regenPerSecond = 3f;

    public float Health { get; private set; }
    public float MaxHealth => maxHealth;
    public bool IsDead { get; private set; }

    private float lastDamageTime = -100f;

    private void Awake()
    {
        Instance = this;
        Health = maxHealth;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (IsDead || Time.timeScale == 0f) return;

        if (Health < maxHealth && Time.time - lastDamageTime > regenDelay)
            Health = Mathf.Min(maxHealth, Health + regenPerSecond * Time.deltaTime);
    }

    public void TakeDamage(float amount)
    {
        if (IsDead || amount <= 0f) return;

        Health = Mathf.Max(0f, Health - amount);
        lastDamageTime = Time.time;

        if (Health <= 0f) Die();
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        Health = Mathf.Min(maxHealth, Health + amount);
    }

    private void Die()
    {
        IsDead = true;

        PlayerController pc = GetComponent<PlayerController>();
        if (pc != null) pc.CanMove = false;

        if (GameManager.Instance != null) GameManager.Instance.GameOver();
        else Debug.LogWarning("PlayerHealth: no GameManager in the scene, so no Game Over screen.");
    }
}
