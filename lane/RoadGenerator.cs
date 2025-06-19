using UnityEngine;

public class RoadGenerator : MonoBehaviour
{
    [Header("Lane Spacing")]
    [Tooltip("Center-to-center distance between the two lanes.")]
    [SerializeField] private float laneSpacing = 3f;

    [Header("Lane Settings")]
    [Tooltip("Width of each lane")]
    [SerializeField] private float laneWidth = 3f;
    [Tooltip("Length of each lane")]
    [SerializeField] public float laneLength = 10f;
    [Tooltip("Height (thickness) of the lane road mesh")]
    [SerializeField] private float laneHeight = 0.1f;
    [Tooltip("Checkpoint collider height")]
    [SerializeField] private float checkpointHeight = 2f;
    [Tooltip("If true, checkpoint colliders are triggers")]
    [SerializeField] private bool isCheckpointTrigger = true;
    [Tooltip("Optional material for both lanes")]
    [SerializeField] private Material laneMaterial;

    [Header("Specific Lane IDs")]
    [Tooltip("If you want to override the ID for Lane #1 (forward)")]
    [SerializeField] private string laneId_Forward;

    [Tooltip("If you want to override the ID for Lane #2 (backward)")]
    [SerializeField] private string laneId_Backward;

    private void Start()
    {
        // Generate roads in all four cardinal directions
        GenerateRoad(Vector3.forward);  // North
    }

    private void GenerateRoad(Vector3 direction)
    {
        // Container for both lanes
        GameObject lanesParent = new GameObject("Lanes");
        lanesParent.transform.SetParent(transform, false);
        
        // Set the rotation based on the desired direction
        lanesParent.transform.rotation = Quaternion.LookRotation(direction);

        // Calculate half-offset using the direction
        Vector3 perp = Vector3.Cross(direction, Vector3.up).normalized;
        Vector3 halfOffset = perp * (laneSpacing * 0.5f);

        // ------ FORWARD (OUTGOING) LANE ------
        GameObject forwardLaneGO = new GameObject("ForwardLane");
        forwardLaneGO.transform.SetParent(lanesParent.transform, false);
        forwardLaneGO.transform.position = transform.position + halfOffset;
        forwardLaneGO.transform.rotation = lanesParent.transform.rotation;

        RoadLaneGenerator forwardLane = forwardLaneGO.AddComponent<RoadLaneGenerator>();
        forwardLane.laneWidth = laneWidth;
        forwardLane.laneLength = laneLength;
        forwardLane.laneHeight = laneHeight;
        forwardLane.checkpointHeight = checkpointHeight;
        forwardLane.isCheckpointTrigger = isCheckpointTrigger;
        forwardLane.laneMaterial = laneMaterial;

        if (!string.IsNullOrEmpty(laneId_Forward))
        {
            forwardLane.laneId = laneId_Forward;
        }

        // ------ BACKWARD (INCOMING) LANE ------
        GameObject backwardLaneGO = new GameObject("BackwardLane");
        backwardLaneGO.transform.SetParent(lanesParent.transform, false);
        backwardLaneGO.transform.position = transform.position - halfOffset;
        backwardLaneGO.transform.rotation = lanesParent.transform.rotation * Quaternion.Euler(0, 180, 0);

        RoadLaneGenerator backwardLane = backwardLaneGO.AddComponent<RoadLaneGenerator>();
        backwardLane.laneWidth = laneWidth;
        backwardLane.laneLength = laneLength;
        backwardLane.laneHeight = laneHeight;
        backwardLane.checkpointHeight = checkpointHeight;
        backwardLane.isCheckpointTrigger = isCheckpointTrigger;
        backwardLane.laneMaterial = laneMaterial;

        if (!string.IsNullOrEmpty(laneId_Backward))
        {
            backwardLane.laneId = laneId_Backward;
        }
    }

    private void OnDrawGizmos()
    {
        // Update gizmo to use transform.forward
        Gizmos.color = Color.blue;
        Vector3 center = transform.position;
        float arrowLen = 2f;

        Gizmos.DrawLine(center, center + transform.forward * arrowLen);

        // Arrowhead
        float headSize = 0.5f;
        Vector3 right = Quaternion.Euler(0, 30, 0) * -transform.forward;
        Vector3 left  = Quaternion.Euler(0, -30, 0) * -transform.forward;

        Gizmos.DrawLine(
            center + transform.forward * arrowLen,
            center + transform.forward * arrowLen + right.normalized * headSize
        );
        Gizmos.DrawLine(
            center + transform.forward * arrowLen,
            center + transform.forward * arrowLen + left.normalized * headSize
        );

        // Show lane spacing in yellow
        Gizmos.color = Color.yellow;
        Vector3 halfOffset = Vector3.Cross(transform.forward, Vector3.up).normalized * (laneSpacing * 0.5f);
        Gizmos.DrawLine(center + halfOffset, center - halfOffset);
    }
}