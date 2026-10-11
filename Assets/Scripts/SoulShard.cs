using UnityEngine;

public class SoulShard : MonoBehaviour
{
    public float spinSpeed = 90f;

    void Update()
    {
        transform.Rotate(0, spinSpeed * Time.deltaTime, 0);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Shard touched by: " + other.name);

        if (!other.CompareTag("Player") && !other.transform.root.CompareTag("Player")) return;

        if (GateObjective.Instance == null)
        {
            Debug.LogError("No GateObjective in the scene!");
            return;
        }

        Debug.Log("Shard picked up");
        GateObjective.Instance.AddShard();
        Destroy(gameObject);
    }
}