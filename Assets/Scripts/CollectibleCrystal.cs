using UnityEngine;

// Put this on an empty object that has a trigger collider.
// The visible crystal model should be a child of this object.
public class CollectibleCrystal : MonoBehaviour
{
    [SerializeField] private float energyValue = 10f;
    [SerializeField] private float spinSpeed = 60f;
    [SerializeField] private float bobHeight = 0.15f;
    [SerializeField] private float bobSpeed = 2f;

    private Vector3 startPosition;
    private bool collected;

    private void Start()
    {
        startPosition = transform.position;
        Collider c = GetComponent<Collider>();
        if (c == null) Debug.LogError("CollectibleCrystal: add a trigger collider to " + name);
        else if (!c.isTrigger) Debug.LogWarning("CollectibleCrystal: collider on " + name + " should be 'Is Trigger'.");
    }

    private void Update()
    {
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
        transform.position = startPosition + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobHeight);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag("Player")) return;
        collected = true;

        if (MagicSystem.Instance != null)
            MagicSystem.Instance.CollectCrystal(energyValue);

        Destroy(gameObject);
    }
}
