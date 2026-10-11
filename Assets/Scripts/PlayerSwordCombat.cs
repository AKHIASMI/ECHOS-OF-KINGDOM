using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

// Press F (and optionally left mouse) to swing the sword at demons in front of the player.
// Put this on the Player object. It animates the knight avatar's "Sword" and "Body" if they exist.
public class PlayerSwordCombat : MonoBehaviour
{
    public float damage = 25f;
    public float range = 2.6f;
    public float arcDegrees = 110f;
    public float cooldown = 0.55f;
    public float hitDelay = 0.14f;       // seconds into the swing when the blow lands
    public float swingDuration = 0.35f;
    public bool useLeftMouse = false;    // off by default so it will not clash with spell casting

    private Transform sword;
    private Transform body;
    private Vector3 swordPos;
    private Quaternion swordRot;
    private Quaternion bodyRot;
    private float nextAttack;
    private bool swinging;

    private void Start()
    {
        Transform avatar = transform.Find("Avatar");
        if (avatar != null)
        {
            body = avatar.Find("Body");
            sword = avatar.Find("Body/Sword");
        }

        if (sword != null)
        {
            swordPos = sword.localPosition;
            swordRot = sword.localRotation;
        }
        if (body != null) bodyRot = body.localRotation;
    }

    private void Update()
    {
        if (Time.timeScale == 0f || swinging || Time.time < nextAttack) return;

        PlayerHealth health = PlayerHealth.Instance;
        if (health != null && health.IsDead) return;

        if (AttackPressed()) StartCoroutine(Swing());
    }

    private bool AttackPressed()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        bool key = Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame;
        bool mouse = useLeftMouse && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        return key || mouse;
#else
        return Input.GetKeyDown(KeyCode.F) || (useLeftMouse && Input.GetMouseButtonDown(0));
#endif
    }

    private IEnumerator Swing()
    {
        swinging = true;
        nextAttack = Time.time + cooldown;

        bool hitDone = false;
        float t = 0f;
        while (t < swingDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / swingDuration);
            float k = Mathf.Sin(p * Mathf.PI);

            if (sword != null)
            {
                sword.localPosition = swordPos + new Vector3(-0.1f, 0f, 0.75f) * k;
                sword.localRotation = swordRot * Quaternion.Euler(95f * k, 0f, 0f);
            }
            if (body != null) body.localRotation = bodyRot * Quaternion.Euler(0f, -30f * k, 0f);

            if (!hitDone && t >= hitDelay)
            {
                hitDone = true;
                DealDamage();
            }
            yield return null;
        }

        if (sword != null)
        {
            sword.localPosition = swordPos;
            sword.localRotation = swordRot;
        }
        if (body != null) body.localRotation = bodyRot;
        swinging = false;
    }

    private void DealDamage()
    {
        Vector3 origin = transform.position;
        Vector3 fwd = transform.forward;
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 0.001f) fwd = Vector3.forward;
        fwd.Normalize();

        // Reverse loop: a kill removes the demon from the list while we iterate.
        for (int i = DemonHealth.All.Count - 1; i >= 0; i--)
        {
            DemonHealth d = DemonHealth.All[i];
            if (d == null || d.IsDead) continue;

            Vector3 to = d.transform.position - origin;
            to.y = 0f;
            float dist = to.magnitude;

            if (dist > range + d.bodyRadius) continue;
            if (dist > 0.6f && Vector3.Angle(fwd, to) > arcDegrees * 0.5f) continue;

            d.TakeDamage(damage, origin);
        }
    }
}
