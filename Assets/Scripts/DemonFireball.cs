using UnityEngine;

public class DemonFireball : MonoBehaviour
{
    public float speed = 12f;
    public float damage = 10f;
    public float lifeTime = 5f;

    Vector3 direction = Vector3.forward;

    public static DemonFireball Spawn(Vector3 position, Vector3 direction)
    {
        return Spawn(position, direction, 12f, 10f);
    }

    public static DemonFireball Spawn(Vector3 position, Vector3 direction, float speed)
    {
        return Spawn(position, direction, speed, 10f);
    }

    public static DemonFireball Spawn(Vector3 position, Vector3 direction, float speed, float damage)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "DemonFireball";
        go.transform.position = position;
        go.transform.localScale = Vector3.one * 0.5f;

        go.GetComponent<Collider>().isTrigger = true;

        Rigidbody rb = go.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        go.GetComponent<Renderer>().material.color = new Color(1f, 0.3f, 0f);

        DemonFireball fb = go.AddComponent<DemonFireball>();
        fb.direction = direction.normalized;
        fb.speed = speed;
        fb.damage = damage;
        return fb;
    }

    public static DemonFireball Launch(Vector3 position, Vector3 direction)
    {
        return Spawn(position, direction);
    }

    public static DemonFireball Launch(Vector3 position, Vector3 direction, float speed, float damage)
    {
        return Spawn(position, direction, speed, damage);
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;
        if (other.GetComponentInParent<DemonAI>() != null) return;

        if (other.CompareTag("Player") || other.transform.root.CompareTag("Player"))
        {
            other.transform.root.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        }

        Destroy(gameObject);
    }
}