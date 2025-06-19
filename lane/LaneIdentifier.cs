using UnityEngine;

public class LaneIdentifier : MonoBehaviour
{
    public string LaneId { get; private set; }

    public void SetLaneId(string id)
    {
        LaneId = id;
    }
} 