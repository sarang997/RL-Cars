using UnityEngine;

public class CheckpointInfo : MonoBehaviour
{
    private string laneID;
    private string checkpointType; // "entrance" or "exit"

    public void Initialize(string laneID, string checkpointType)
    {
        this.laneID = laneID;
        this.checkpointType = checkpointType;
    }

    public string GetLaneID()
    {
        return laneID;
    }

    public string GetCheckpointType()
    {
        return checkpointType;
    }
}