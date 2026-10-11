using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

// Put this on the demon (next to DemonAI). DemonAI raises the max health on higher levels.
// Draws a small health bar above the demon, no UI setup needed.
public class DemonHealth : MonoBehaviour
{
    public static readonly List<DemonHealth> All = new List<DemonHealth>();

    [SerializeField] private float baseMaxHealth = 100f;
    [SerializeField] private Vector3 barOffset = new Vector3(0f, 2.8f, 0f);
    [SerializeField] private float barShowDistance = 30f;
    [SerializeField] private float knockback = 0.4f;

    // How wide the demon is. The sword adds this to its reach so big demons are easier to hit.
    public float bodyRadius = 0.8f;

    public UnityEvent onDeath;

    public float BaseMaxHealth => baseMaxHealth;
    public float Health { get; private set; }
    public float MaxHealth { get; private set; }
    public bool IsDead { get; private set; }
    public float Fraction => MaxHealth > 0f ? Health / MaxHealth : 0f;

    private void Awake()
    {
        MaxHealth = baseMaxHealth;
        Health = MaxHealth;
    }

    private void OnEnable()
    {
        if (!All.Contains(this)) All.Add(this);
    }

    private void OnDisable()
    {
        All.Remove(this);
    }

    public void SetMaxHealth(float value)
    {
        MaxHealth = value;
        Health = value;
    }

    public void TakeDamage(float amount)
    {
        if (IsDead || amount <= 0f) return;

        Health = Mathf.Max(0f, Health - amount);
        if (Health <= 0f) Die();
    }

    // Used by the sword: damage plus a small shove away from where the hit came from.
    public void TakeDamage(float amount, Vector3 hitFrom)
    {
        TakeDamage(amount);
        if (IsDead || knockback <= 0f) return;

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;

        Vector3 away = transform.position - hitFrom;
        away.y = 0f;
        if (away.sqrMagnitude < 0.001f) return;
        agent.Move(away.normalized * knockback);
    }

    private void Die()
    {
        IsDead = true;
        onDeath.Invoke();

        DemonAI ai = GetComponent<DemonAI>();
        if (ai != null) ai.enabled = false;

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        foreach (Collider c in GetComponentsInChildren<Collider>())
            c.enabled = false;

        if (MagicSystem.Instance != null)
            MagicSystem.Instance.PostMessage("The demon is defeated!", 3f);

        StartCoroutine(SinkAndRemove());
    }

    private IEnumerator SinkAndRemove()
    {
        yield return new WaitForSeconds(1.5f);

        Vector3 start = transform.position;
        float t = 0f;
        while (t < 2f)
        {
            t += Time.deltaTime;
            transform.position = start + Vector3.down * t * 0.8f;
            yield return null;
        }
        Destroy(gameObject);
    }

    private void OnGUI()
    {
        if (IsDead) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 screen = cam.WorldToScreenPoint(transform.position + barOffset);
        if (screen.z <= 0f || screen.z > barShowDistance) return;

        const float w = 90f;
        const float h = 10f;
        float x = screen.x - w * 0.5f;
        float y = Screen.height - screen.y;

        GUI.color = Color.black;
        GUI.DrawTexture(new Rect(x - 2f, y - 2f, w + 4f, h + 4f), Texture2D.whiteTexture);
        GUI.color = new Color(0.8f, 0.05f, 0.05f);
        GUI.DrawTexture(new Rect(x, y, w * Fraction, h), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }
}
