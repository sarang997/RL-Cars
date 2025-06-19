using UnityEngine;

public class ShowcaseLightUpdater : MonoBehaviour
{
    [Header("Traffic Light Controller Reference")]
    [SerializeField] private TrafficLightController trafficLightController; // Reference to the TrafficLightController on the cube

    private Renderer sphereRenderer; // The renderer of this sphere

    void Start()
    {
        // Get the Renderer component of the sphere this script is attached to
        sphereRenderer = GetComponent<Renderer>();

        if (trafficLightController == null)
        {
            Debug.LogError("Traffic Light Controller reference is not assigned. Please assign it in the Inspector.");
        }

        if (sphereRenderer == null)
        {
            Debug.LogError("No Renderer found on the sphere. Please make sure this object has a Renderer component.");
        }
    }

    void Update()
    {
        if (trafficLightController != null && sphereRenderer != null)
        {
            UpdateSphereColor();
        }
    }

    private void UpdateSphereColor()
    {
        // Determine the color based on the traffic light's current state
        Color newColor = (trafficLightController.currentState == TrafficLightController.State.Red)
            ? Color.red
            : Color.green;

        sphereRenderer.material.color = newColor;
    }
}