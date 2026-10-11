using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.6f, 0f);

    [Header("Orbit")]
    [SerializeField] private float distance = 6f;
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float minPitch = -20f;
    [SerializeField] private float maxPitch = 60f;

    [Header("Collision")]
    [SerializeField] private LayerMask obstacleMask = ~0;
    [SerializeField] private float collisionPadding = 0.3f;

    private float yaw;
    private float pitch = 15f;

    public bool InputEnabled { get; set; } = true;

    private void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
            else Debug.LogError("CameraController: assign a target or tag your player as 'Player'.");
        }

        yaw = transform.eulerAngles.y;
        SetCursorLocked(true);
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // No mouse look while the game is paused
        if (InputEnabled && Time.timeScale > 0f)
        {
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focus = target.position + targetOffset;
        Vector3 desiredPos = focus - rotation * Vector3.forward * distance;

        // Pull the camera in if a wall is between it and the player
        Vector3 dir = desiredPos - focus;
        if (Physics.SphereCast(focus, collisionPadding, dir.normalized, out RaycastHit hit,
                               distance, obstacleMask, QueryTriggerInteraction.Ignore)
            && !hit.transform.IsChildOf(target))
        {
            desiredPos = focus + dir.normalized * Mathf.Max(0.5f, hit.distance);
        }

        transform.SetPositionAndRotation(desiredPos, rotation);
    }

    public static void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
