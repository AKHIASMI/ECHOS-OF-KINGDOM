using UnityEngine;

// Put this on the human model (a child of the Player). It watches how fast the Player is moving
// and feeds that to the Animator's "Speed" parameter, so it works with any movement script.
[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Transform player; // leave empty to use the parent object

    private Animator animator;
    private Vector3 lastPosition;

    private void Start()
    {
        animator = GetComponent<Animator>();
        if (player == null) player = transform.parent;

        if (player == null)
        {
            Debug.LogWarning("PlayerAnimator: put the human model as a child of the Player object.");
            enabled = false;
            return;
        }
        lastPosition = player.position;
    }

    private void Update()
    {
        if (Time.deltaTime <= 0f) return;

        Vector3 delta = player.position - lastPosition;
        delta.y = 0f;
        lastPosition = player.position;

        float speed = delta.magnitude / Time.deltaTime;
        animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);
    }
}
