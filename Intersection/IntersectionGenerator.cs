using UnityEngine;

public class IntersectionGenerator : MonoBehaviour
{
    [Header("Road Settings")]
    [Tooltip("Overall direction the road extends in (will be normalized)")]
    [SerializeField] private Vector3 roadDirection = Vector3.forward;
    
    [Tooltip("Width of each lane in meters")]
    [SerializeField] private float laneWidth = 3f;

    private void Start()
    {
        GenerateRoad();
    }

    private void GenerateRoad()
    {
        roadDirection = roadDirection.normalized;
        
        // Create a road container
        GameObject roadContainer = new GameObject("Road_Section");
        roadContainer.transform.SetParent(transform);
        roadContainer.transform.localPosition = Vector3.zero;

        // Calculate lane offset (half lane width for proper spacing)
        float laneOffset = laneWidth / 2f;
        // Calculate the right vector relative to the road direction
        Vector3 rightOffset = Vector3.Cross(roadDirection, Vector3.up).normalized * laneOffset;

        // Create lanes with proper naming and direction
        CreateLane(roadContainer, "Lane_Right", transform.position + rightOffset, -roadDirection); // Incoming
        CreateLane(roadContainer, "Lane_Left", transform.position - rightOffset, roadDirection);   // Outgoing
    }

    private void CreateLane(GameObject parent, string name, Vector3 position, Vector3 direction)
    {
        GameObject lane = new GameObject(name);
        lane.transform.SetParent(parent.transform);
        lane.transform.position = position;
        
        RoadLaneGenerator laneGenerator = lane.AddComponent<RoadLaneGenerator>();
        SetLaneDirection(laneGenerator, direction);
    }

    private void SetLaneDirection(RoadLaneGenerator generator, Vector3 direction)
    {
        var serializedObject = new UnityEditor.SerializedObject(generator);
        var laneDirectionProperty = serializedObject.FindProperty("laneDirection");
        laneDirectionProperty.vector3Value = direction;
        serializedObject.ApplyModifiedProperties();
    }

    private void OnDrawGizmos()
    {
        // Draw the overall road direction
        Gizmos.color = Color.blue;
        Vector3 direction = roadDirection.normalized;
        Vector3 center = transform.position;
        Gizmos.DrawLine(center, center + direction * 2f); // Just draw a short direction indicator
        
        // Draw arrow head
        Vector3 right = Quaternion.Euler(0, 30, 0) * -direction;
        Vector3 left = Quaternion.Euler(0, -30, 0) * -direction;
        Gizmos.DrawLine(center + direction * 2f, center + direction * 2f + right * 0.5f);
        Gizmos.DrawLine(center + direction * 2f, center + direction * 2f + left * 0.5f);
    }
}