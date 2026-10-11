using UnityEngine;

// Put this on the Player. Finds the nearest Interactable and uses it when E is pressed.
public class PlayerInteraction : MonoBehaviour
{
    public static PlayerInteraction Instance { get; private set; }

    [SerializeField] private float interactRadius = 2.5f;

    public Interactable Current { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (Time.timeScale == 0f)
        {
            Current = null;
            return;
        }

        Current = FindNearest();

        if (Current != null && Input.GetKeyDown(KeyCode.E))
            Current.Interact();
    }

    private Interactable FindNearest()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactRadius,
                                                ~0, QueryTriggerInteraction.Collide);
        Interactable best = null;
        float bestDist = float.MaxValue;

        foreach (Collider hit in hits)
        {
            Interactable candidate = hit.GetComponentInParent<Interactable>();
            if (candidate == null || !candidate.CanInteract) continue;

            float d = (candidate.transform.position - transform.position).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                best = candidate;
            }
        }
        return best;
    }
}
