using UnityEngine;

public class TrafficLightController : MonoBehaviour
{
    public enum State { Red, Green }
    public State currentState = State.Red;

    private Renderer lightRenderer;

    [Header("Training Mode")]
    [SerializeField] private bool isTrainingMode = false;
    [SerializeField] private bool alwaysRed = true;

    public bool IsStopped 
    { 
        get 
        {
            if (isTrainingMode)
                return alwaysRed;
            return currentState == State.Red;
        }
    }

    void Start()
    {
        lightRenderer = GetComponent<Renderer>();
        if (lightRenderer == null)
        {
            Debug.LogError("No Renderer found! Make sure this object has a Renderer component.");
            return;
        }
        UpdateLightColor();
    }

    void Update()
    {
        UpdateLightColor();
    }

    private void SwitchState(State newState)
    {
        currentState = newState;
        UpdateLightColor();
        Debug.Log($"Traffic Light switched to: {currentState}");
    }

    private void UpdateLightColor()
    {
        if (lightRenderer != null)
        {
            Color newColor;
            if (isTrainingMode)
            {
                newColor = alwaysRed ? Color.red : Color.green;
            }
            else
            {
                newColor = (currentState == State.Red) ? Color.red : Color.green;
            }
            newColor.a = 0.5f;
            lightRenderer.material.color = newColor;
        }
    }
}