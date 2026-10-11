using UnityEngine;

// Slides a door/gate up when opened.
public class DoorController : MonoBehaviour
{
    public Vector3 openOffset = new Vector3(0f, 6.2f, 0f);
    public float speed = 2f;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpen;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + openOffset;
    }

    public void Open() { isOpen = true; }
    public void Close() { isOpen = false; }

    private void Update()
    {
        Vector3 target = isOpen ? openPosition : closedPosition;
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }
}
