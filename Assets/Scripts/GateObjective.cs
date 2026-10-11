using UnityEngine;

public class GateObjective : MonoBehaviour
{
    public static GateObjective Instance;

    public int shardsRequired = 3;
    public Transform gate;
    public Vector3 openOffset = new Vector3(0, 8, 0);
    public float openSpeed = 2f;

    int collected;
    bool open;
    Vector3 closedPos;

    public bool IsOpen => open;

    void Awake()
    {
        Instance = this;
        closedPos = gate.position;
    }

    public void AddShard()
    {
        collected++;
        Debug.Log("Shards: " + collected + "/" + shardsRequired);
        if (collected >= shardsRequired) open = true;
    }

    void Update()
    {
        if (open)
            gate.position = Vector3.MoveTowards(gate.position, closedPos + openOffset, openSpeed * Time.deltaTime);
    }
}