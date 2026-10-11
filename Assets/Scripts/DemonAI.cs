using System.Collections;
using UnityEngine;
using UnityEngine.AI;

// A demon that patrols, chases the player, attacks, and gets stronger on every level.
// - Level 1 uses the base numbers below. The LAST level uses the "final" multipliers.
// - From level 2 the demon also throws fireballs.
// - Below 50% health the demon becomes enraged.
// It never targets the player while the player is inside a SafeZone.
[RequireComponent(typeof(NavMeshAgent))]
public class DemonAI : MonoBehaviour
{
    [Header("Base stats (Level 1). Higher levels multiply these.")]
    public float detectRange = 14f;
    public float loseRange = 22f;
    public float attackRange = 1.9f;
    public float attackDamage = 12f;
    public float attackCooldown = 1.3f;
    public float chaseSpeed = 4.5f;
    public float patrolSpeed = 1.8f;
    public float patrolRadius = 8f;
    public Transform visual;

    [Header("How strong on the LAST level (1 = same as level 1)")]
    public float finalHealthMultiplier = 3f;
    public float finalDamageMultiplier = 1.6f;
    public float finalSpeedMultiplier = 1.3f;
    public float finalCooldownMultiplier = 0.75f; // smaller = attacks faster
    public float finalRangeMultiplier = 1.6f;

    [Header("Fireball (unlocked from level 2)")]
    public float fireballCooldown = 4f;
    public float fireballSpeed = 11f;
    public float fireballDamage = 10f;
    public float fireballMinRange = 5f;
    public float fireballMaxRange = 16f;

    private NavMeshAgent agent;
    private DemonHealth health;
    private Transform player;
    private Vector3 home;
    private bool chasing;
    private bool lunging;
    private bool warned;
    private bool enraged;
    private bool fireballsUnlocked;
    private float nextAttack;
    private float nextPatrol;
    private float nextFireball;

    // Values actually used in play (base values x level multipliers)
    private float curDetect, curLose, curDamage, curCooldown, curChase;
    private float curFireballCooldown, curFireballDamage;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<DemonHealth>();
        home = transform.position;

        ApplyLevelScaling();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        else Debug.LogError("DemonAI: no object tagged 'Player' found.");

        // Move onto the navigation mesh before turning the agent on
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 8f, NavMesh.AllAreas))
            transform.position = hit.position;
        else
            Debug.LogWarning("DemonAI: no navigation mesh found near the demon. Check the NavMesh_Baker object exists.");

        agent.stoppingDistance = attackRange * 0.7f;
        agent.enabled = true;
    }

    private void ApplyLevelScaling()
    {
        int level = Mathf.Max(1, LevelManager.CurrentLevelIndex);
        int count = Mathf.Max(1, LevelManager.LevelCount);
        float t = count <= 1 ? 0f : Mathf.Clamp01((level - 1f) / (count - 1f));

        curDetect = detectRange * Mathf.Lerp(1f, finalRangeMultiplier, t);
        curLose = loseRange * Mathf.Lerp(1f, finalRangeMultiplier, t);
        curDamage = attackDamage * Mathf.Lerp(1f, finalDamageMultiplier, t);
        curCooldown = attackCooldown * Mathf.Lerp(1f, finalCooldownMultiplier, t);
        curChase = chaseSpeed * Mathf.Lerp(1f, finalSpeedMultiplier, t);
        curFireballCooldown = fireballCooldown * Mathf.Lerp(1f, finalCooldownMultiplier, t);
        curFireballDamage = fireballDamage * Mathf.Lerp(1f, finalDamageMultiplier, t);
        fireballsUnlocked = level >= 2;

        if (health != null)
            health.SetMaxHealth(health.BaseMaxHealth * Mathf.Lerp(1f, finalHealthMultiplier, t));
    }

    private void Enrage()
    {
        enraged = true;
        curChase *= 1.15f;
        curDamage *= 1.25f;
        curCooldown *= 0.85f;
        curFireballCooldown *= 0.7f;
        if (visual != null) visual.localScale *= 1.1f;
        if (MagicSystem.Instance != null) MagicSystem.Instance.PostMessage("The demon is enraged!", 3f);
    }

    private void Update()
    {
        if (Time.timeScale == 0f || player == null || agent == null || !agent.enabled) return;
        if (health != null && health.IsDead) return;

        if (!agent.isOnNavMesh)
        {
            if (!warned)
            {
                warned = true;
                Debug.LogWarning("DemonAI: the demon is not on the navigation mesh, so it cannot move.");
            }
            return;
        }

        if (!enraged && health != null && health.Fraction <= 0.5f) Enrage();

        PlayerHealth playerHealth = PlayerHealth.Instance;
        bool playerOut = (playerHealth != null && playerHealth.IsDead)
                         || (QuestManager.Instance != null && QuestManager.Instance.LevelComplete)
                         || SafeZone.IsSafe(player.position);

        float dist = FlatDistance(transform.position, player.position);

        if (playerOut) chasing = false;
        else if (!chasing && dist <= curDetect) chasing = true;
        else if (chasing && dist > curLose) chasing = false;

        if (chasing) Chase(dist, playerHealth);
        else Patrol();

        KeepOutOfSafeZones();
    }

    private void Chase(float dist, PlayerHealth playerHealth)
    {
        agent.speed = curChase;
        agent.isStopped = false;
        agent.SetDestination(player.position);

        if (dist <= attackRange)
        {
            FacePlayer(10f);

            if (Time.time >= nextAttack)
            {
                nextAttack = Time.time + curCooldown;
                Attack(playerHealth);
            }
        }
        else if (fireballsUnlocked && dist >= fireballMinRange && dist <= fireballMaxRange
                 && Time.time >= nextFireball && CanSeePlayer())
        {
            nextFireball = Time.time + curFireballCooldown;
            ShootFireball();
        }
    }

    private void FacePlayer(float turnSpeed)
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), turnSpeed * Time.deltaTime);
    }

    private bool CanSeePlayer()
    {
        Vector3 from = transform.position + Vector3.up * 1.5f;
        Vector3 to = player.position + Vector3.up * 1f;
        RaycastHit hit;
        if (Physics.Linecast(from, to, out hit, ~0, QueryTriggerInteraction.Ignore))
            return hit.transform.IsChildOf(player) || hit.transform.IsChildOf(transform);
        return true;
    }

    private void ShootFireball()
    {
        FacePlayer(20f);
        Vector3 from = transform.position + Vector3.up * 1.5f;
        Vector3 to = player.position + Vector3.up * 1f;
        DemonFireball.Spawn(from, to, fireballSpeed, curFireballDamage);
        if (MagicSystem.Instance != null) MagicSystem.Instance.PostMessage("The demon hurls a fireball!", 1.5f);
    }

    private void Patrol()
    {
        agent.speed = patrolSpeed;
        agent.isStopped = false;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.3f && Time.time >= nextPatrol)
        {
            nextPatrol = Time.time + Random.Range(2f, 5f);
            Vector3 target = home + new Vector3(Random.Range(-patrolRadius, patrolRadius), 0f,
                                                Random.Range(-patrolRadius, patrolRadius));
            NavMeshHit hit;
            if (NavMesh.SamplePosition(target, out hit, 4f, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
        }
    }

    private void Attack(PlayerHealth playerHealth)
    {
        if (playerHealth != null) playerHealth.TakeDamage(curDamage);
        if (MagicSystem.Instance != null) MagicSystem.Instance.PostMessage("The demon strikes you!", 1.5f);
        StartCoroutine(Lunge());
    }

    private IEnumerator Lunge()
    {
        if (visual == null || lunging) yield break;
        lunging = true;

        Vector3 start = visual.localPosition;
        float t = 0f;
        const float duration = 0.25f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Sin(Mathf.Clamp01(t / duration) * Mathf.PI);
            visual.localPosition = start + Vector3.forward * 0.5f * k;
            yield return null;
        }
        visual.localPosition = start;
        lunging = false;
    }

    // Backup: if the demon ever ends up inside a safe zone, move it out.
    private void KeepOutOfSafeZones()
    {
        Vector3 pushed;
        if (!SafeZone.TryPushOut(transform.position, out pushed)) return;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(pushed, out hit, 3f, NavMesh.AllAreas))
            agent.Warp(hit.position);
    }

    private static float FlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }
}
