using UnityEngine;

// Swings the legs and arms of the primitive-built knight avatar while the player moves.
public class PlayerAvatarAnimator : MonoBehaviour
{
    public Transform legL;
    public Transform legR;
    public Transform armL;
    public Transform armR;
    public Transform body;

    [SerializeField] private float swingAngle = 40f;
    [SerializeField] private float swingSpeed = 9f;

    private CharacterController controller;
    private Vector3 lastPosition;
    private Vector3 bodyStart;
    private float phase;
    private float amount;

    private void Start()
    {
        controller = GetComponentInParent<CharacterController>();
        lastPosition = transform.position;
        if (body != null) bodyStart = body.localPosition;
    }

    private void Update()
    {
        if (Time.deltaTime <= 0f) return;

        Vector3 delta = transform.position - lastPosition;
        delta.y = 0f;
        float speed = delta.magnitude / Time.deltaTime;
        lastPosition = transform.position;

        bool grounded = controller == null || controller.isGrounded;
        float target = Mathf.Clamp01(speed / 7f);
        amount = Mathf.Lerp(amount, target, Time.deltaTime * 10f);
        if (speed > 0.1f) phase += Time.deltaTime * swingSpeed * (0.6f + amount);

        float swing = Mathf.Sin(phase) * swingAngle * amount;

        if (legL != null) legL.localRotation = Quaternion.Euler(swing, 0f, 0f);
        if (legR != null) legR.localRotation = Quaternion.Euler(-swing, 0f, 0f);

        // Arms swing while walking, and lift a little while in the air
        float lift = grounded ? 0f : -50f;
        if (armL != null) armL.localRotation = Quaternion.Euler(-swing + lift, 0f, 0f);
        if (armR != null) armR.localRotation = Quaternion.Euler(swing + lift, 0f, 0f);

        if (body != null)
            body.localPosition = bodyStart + Vector3.up * (Mathf.Abs(Mathf.Sin(phase)) * 0.05f * amount);
    }
}
