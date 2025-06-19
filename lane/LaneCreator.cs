using UnityEngine;

public class LaneWithCheckpoints : MonoBehaviour
{
    [Header("Lane Settings")]
    public GameObject lane;  // Assign the plane manually from the Scene
    public string laneID = ""; // Unique ID for the lane (optional: auto-generate)
    public bool autoGenerateID = true; // Automatically generate unique lane ID if empty
    public Vector3 directionVector = Vector3.forward; // Lane's forward direction (default)

    [Header("Checkpoint Settings")]
    public GameObject checkpointPrefab;  // Assign a prefab for the checkpoints
    public float checkpointHeight = 0.5f; // Height to position the checkpoints
    public Material checkpointMaterial;  // Optional material for the checkpoints
    public bool addWrongLaneCheckpoint = true; // Option to add wrong-lane checkpoint

    void Start()
    {
        if (lane == null)
        {
            Debug.LogError("No lane assigned! Please attach a manually created plane.");
            return;
        }

        // Assign lane properties
        AssignLaneProperties();

        // Add checkpoints
        AddCheckpoints();
    }

    void AssignLaneProperties()
    {
        // Generate a unique ID if auto-generation is enabled and no ID is provided
        if (autoGenerateID && string.IsNullOrEmpty(laneID))
        {
            laneID = System.Guid.NewGuid().ToString();
        }

        // Set the lane's name and tag
        lane.name = $"Lane_{laneID}";
        lane.tag = "Road";

        // Log the lane direction for debugging
        Debug.Log($"Lane {laneID} has Direction Vector: {directionVector}");
    }

    void AddCheckpoints()
    {
        if (checkpointPrefab == null)
        {
            Debug.LogError("No checkpoint prefab assigned! Please attach a prefab for checkpoints.");
            return;
        }

        // Calculate positions for checkpoints based on lane dimensions
        Vector3 laneCenter = lane.transform.position;
        float laneLength = lane.transform.localScale.x * 10f; // Length of the lane
        float laneWidth = lane.transform.localScale.z * 10f;  // Width of the lane

        // Entrance checkpoint (reward checkpoint)
        Vector3 entrancePosition = laneCenter - directionVector * (laneLength / 2);
        CreateCheckpoint(entrancePosition, laneWidth, $"EntranceCheckpoint_{laneID}", true);

        // Exit checkpoint (wrong-lane checkpoint)
        if (addWrongLaneCheckpoint)
        {
            Vector3 exitPosition = laneCenter + directionVector * (laneLength / 2);
            CreateCheckpoint(exitPosition, laneWidth, $"WrongLaneCheckpoint_{laneID}", false);
        }
    }

    void CreateCheckpoint(Vector3 position, float width, string checkpointName, bool isRewardCheckpoint)
    {
        // Instantiate the checkpoint
        GameObject checkpoint = Instantiate(checkpointPrefab, position + Vector3.up * checkpointHeight, Quaternion.identity, this.transform);
        checkpoint.name = checkpointName;

        // Scale the checkpoint to match the lane width
        checkpoint.transform.localScale = new Vector3(width, 0.1f, 0.5f); // Thin and wide across the road

        // Assign material if provided
        if (checkpointMaterial != null)
        {
            Renderer renderer = checkpoint.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = checkpointMaterial;
            }
        }

        // Tag the checkpoint for logic
        if (isRewardCheckpoint)
        {
            checkpoint.tag = "RewardCheckpoint";
        }
        else
        {
            checkpoint.tag = "WrongLaneCheckpoint";
        }

        Debug.Log($"Created {checkpointName} at {position}");
    }
}