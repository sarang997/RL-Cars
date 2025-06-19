using UnityEngine;
using System.Collections.Generic;

public class JunctionManager : MonoBehaviour
{
    [System.Serializable]
    public class TrafficLightGroup
    {
        public List<TrafficLightController> lights;
    }

    [SerializeField] private List<TrafficLightGroup> lightGroups = new List<TrafficLightGroup>();
    [SerializeField] private float greenDuration = 5f;
    
    private int currentGroupIndex = 0;
    private float timer = 0f;

    void Start()
    {
        if (lightGroups.Count == 0) return;

        // Remove any empty groups
        lightGroups.RemoveAll(group => group.lights == null || group.lights.Count == 0);
        
        // Set all groups to red initially
        foreach (var group in lightGroups)
        {
            SetGroupState(group, TrafficLightController.State.Red);
        }
        
        // Set first group to green
        SetGroupState(lightGroups[0], TrafficLightController.State.Green);
    }

    void Update()
    {
        if (lightGroups.Count == 0) return;

        timer += Time.deltaTime;

        if (timer >= greenDuration)
        {
            // Switch to next group
            SetGroupState(lightGroups[currentGroupIndex], TrafficLightController.State.Red);
            
            currentGroupIndex = (currentGroupIndex + 1) % lightGroups.Count;
            SetGroupState(lightGroups[currentGroupIndex], TrafficLightController.State.Green);
            
            timer = 0f;
        }
    }

    private void SetGroupState(TrafficLightGroup group, TrafficLightController.State state)
    {
        if (group == null || group.lights == null) return;
        
        foreach (var light in group.lights)
        {
            if (light != null)
            {
                light.currentState = state;
            }
        }
    }
} 