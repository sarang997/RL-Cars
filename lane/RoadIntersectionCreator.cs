using UnityEngine;

public class RoadIntersectionCreator : MonoBehaviour
{
    public GameObject roadPrefab; // Assign your road prefab here
    public float roadLength = 10f; // Length of the road
    public float spacing = 1f; // Spacing from the center of the intersection

    void Start()
    {
        if (roadPrefab == null)
        {
            Debug.LogError("Road prefab is not assigned!");
            return;
        }

        CreateIntersection();
    }

    void CreateIntersection()
    {
        // Define directions for the roads
        Vector3[] directions = {
            Vector3.forward,  // North
            Vector3.right,    // East
            Vector3.back,     // South
            Vector3.left      // West
        };

        for (int i = 0; i < directions.Length; i++)
        {
            // Calculate position for each road
            Vector3 position = transform.position + directions[i] * (roadLength / 2 + spacing);

            // Instantiate the road prefab
            GameObject road = Instantiate(roadPrefab, position, Quaternion.identity);

            // Rotate the road to align with the direction
            road.transform.rotation = Quaternion.LookRotation(directions[i]);

            // Optionally, parent the road to this GameObject for organization
            road.transform.parent = transform;
        }
    }
}