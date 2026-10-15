using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 7f;
    [SerializeField] private float jumpHeight = 1.3f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float turnSmoothTime = 0.1f;

    [Header("References")]
    [Tooltip("Leave empty to use the Main Camera automatically.")]
    [SerializeField] private Transform cameraTransform;
    [Tooltip("Where the player respawns after falling.")]
    [SerializeField] private Transform respawnPoint;

    [Header("Fall Respawn")]
    [SerializeField] private float fallLimitY = -15f;
    [SerializeField] private float fallDamage = 20f;

    [Header("Audio")]
    [SerializeField] private AudioClip footstepClip;
    [SerializeField, Range(0f, 1f)] private float footstepVolume = 0.5f;

    private CharacterController controller;
    private Vector3 velocity;
    private float turnVelocity;
    private Vector3 fallbackRespawnPosition;
    private AudioSource footstepSource;

    public bool CanMove { get; set; } = true;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        fallbackRespawnPosition = transform.position;

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (cameraTransform == null)
            Debug.LogError("PlayerController: no camera found. Tag your camera as MainCamera.");

        footstepSource = gameObject.AddComponent<AudioSource>();
        footstepSource.clip = footstepClip;
        footstepSource.loop = true;
        footstepSource.playOnAwake = false;
        footstepSource.volume = footstepVolume;
    }

    private void Update()
    {
        if (!CanMove || Time.timeScale == 0f) { SetFootsteps(false, false); return; }

        HandleMovement();

        if (transform.position.y < fallLimitY)
        {
            PlayerHealth health = PlayerHealth.Instance;
            if (health != null)
            {
                health.TakeDamage(fallDamage);
                if (health.IsDead) return;
            }
            Respawn();
        }
    }

    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(h, 0f, v).normalized;

        bool grounded = controller.isGrounded;
        if (grounded && velocity.y < 0f)
            velocity.y = -2f;

        if (input.magnitude >= 0.1f && cameraTransform != null)
        {
            float targetAngle = Mathf.Atan2(input.x, input.z) * Mathf.Rad2Deg
                                + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle,
                                                ref turnVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
            controller.Move(moveDir * speed * Time.deltaTime);
        }

        if (grounded && Input.GetKeyDown(KeyCode.Space))
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        SetFootsteps(input.magnitude >= 0.1f && grounded, Input.GetKey(KeyCode.LeftShift));
    }

    private void SetFootsteps(bool moving, bool running)
    {
        if (footstepSource == null || footstepClip == null) return;

        if (moving)
        {
            footstepSource.pitch = running ? 1.4f : 1f;
            if (!footstepSource.isPlaying) footstepSource.Play();
        }
        else if (footstepSource.isPlaying)
        {
            footstepSource.Stop();
        }
    }

    public void SetRespawnPoint(Transform point)
    {
        respawnPoint = point;
    }

    public void Respawn()
    {
        Vector3 target = respawnPoint != null ? respawnPoint.position : fallbackRespawnPosition;

        controller.enabled = false;
        transform.position = target;
        controller.enabled = true;
        velocity = Vector3.zero;
    }
}