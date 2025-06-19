using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Ezereal;

[RequireComponent(typeof(LatestCarController))]
public class CarAgent : Agent
{
    [Header("Scene References")]
    public GameObject target;    // Target GameObject
    public GameObject ground;    // Ground plane
    public GameObject truck;     // Truck GameObject to reset

    private LatestCarController _carController;
    private Rigidbody _rb;

    private Vector3 _truckInitialPosition;
    private Quaternion _truckInitialRotation;

    [Header("Spawn Settings")]
    public float spawnMargin = 2f;      // Margin from the edges of the ground
    public float checkRadius = 0.5f;      // Check radius for overlap
    public LayerMask obstacleLayer;     // Obstacles to avoid when spawning
    public int maxSpawnTries = 100;     // Maximum retries for finding valid spawn points

    [Header("Reward Settings")]
    public float stepPenalty = -0.0005f; // Penalty per step to encourage movement
    public float targetReward = 10f;     // Reward for hitting the target
    public float wallPenalty = -10f;     // Penalty for hitting a wall
    public float idlePenalty = -0.001f;  // Penalty for staying idle
    public float timeoutPenalty = -5f;   // Penalty for timing out

    private float _lastEpisodeReward = 0f;  // Total reward from the last episode

    [SerializeField]
    private string currentLaneId;  // Changed from int to string
    public string CurrentLaneId 
    { 
        get => currentLaneId;
        set => currentLaneId = value;
    }

    private bool isAtRedLight = false;  // Flag for red light status

    // Control variables
    private float _currentSteer = 0f;
    private float _currentMotor = 0f;
    private float _currentBrake = 0f;

    [Header("Steering Delta Settings")]
    // Instead of an absolute steering value, we now use a steering delta.
    // The agent’s first action will be the incremental change (clamped by maxSteeringDelta).
    public float maxSteeringDelta = 0.1f;          // Maximum steering change allowed per step
    public float steeringDeltaPenaltyFactor = 0.0f;  // Optional: penalize abrupt changes (set to 0 to disable)

    [Header("Smoothing Settings")]
    // For motor and brake, we still use smoothing for a natural transition.
    public float motorSmoothTime = 0.1f;
    public float brakeSmoothTime = 0.05f;
    private float _motorVelocity;
    private float _brakeVelocity;

    [Header("Lane Keeping Settings")]
    public float maxLateralDistance = 2f;       // Maximum acceptable distance from lane center
    public float laneKeepingPenalty = -0.005f;  // Penalty for poor lane positioning
    public float rayLength = 2f;                // Length of the lateral rays
    public LayerMask wallLayer;                 // Layer for walls/barriers

    private RaycastHit leftHit, rightHit;

    public override void Initialize()
    {
        _carController = GetComponent<LatestCarController>();
        _rb = GetComponent<Rigidbody>();

        // Save the truck's initial position and rotation.
        if (truck != null)
        {
            _truckInitialPosition = truck.transform.position;
            _truckInitialRotation = truck.transform.rotation;
        }
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log($"Episode ended with reward: {_lastEpisodeReward}");
        SetReward(0f);

        // Reset the lane ID to default "Outgoing".
        CurrentLaneId = "Outgoing";

        // Reset the truck's position and rotation.
        if (truck != null)
        {
            truck.transform.position = _truckInitialPosition;
            truck.transform.rotation = _truckInitialRotation;
        }

        // Ground-based spawning.
        if (ground != null)
        {
            Renderer groundRenderer = ground.GetComponent<Renderer>();
            Bounds groundBounds = groundRenderer.bounds;
            
            // Randomize the agent’s spawn position.
            Vector3 agentPosition = GetRandomValidPosition(groundBounds);
            transform.position = agentPosition;
            
            // Randomize target’s spawn position if available.
            if (target != null)
            {
                Vector3 targetPosition;
                int attempts = 0;
                do
                {
                    targetPosition = GetRandomValidPosition(groundBounds);
                    float targetHeight = target.GetComponent<Renderer>().bounds.size.y;
                    targetPosition.y += targetHeight / 2;
                    attempts++;
                }
                while (Vector3.Distance(agentPosition, targetPosition) < 5f && attempts < maxSpawnTries);

                target.transform.position = targetPosition;
            }
        }

        // Reset velocity.
        if (_rb != null)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        _lastEpisodeReward = GetCumulativeReward();

        // Reset control variables.
        _currentSteer = 0f;
        _currentMotor = 0f;
        _currentBrake = 0f;
    }

    private Vector3 GetRandomValidPosition(Bounds bounds)
    {
        for (int i = 0; i < maxSpawnTries; i++)
        {
            float x = Random.Range(bounds.min.x + spawnMargin, bounds.max.x - spawnMargin);
            float z = Random.Range(bounds.min.z + spawnMargin, bounds.max.z - spawnMargin);
            float y = transform.position.y; // Keep the y position constant

            Vector3 candidate = new Vector3(x, y, z);

            // Check if the candidate is free of obstacles.
            if (!Physics.CheckSphere(candidate, checkRadius, obstacleLayer))
            {
                return candidate;
            }
        }

        Debug.LogWarning("Failed to find a valid spawn position. Using ground center.");
        return bounds.center;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Observe local velocity.
        if (_rb != null)
        {
            Vector3 localVelocity = transform.InverseTransformDirection(_rb.linearVelocity);
            sensor.AddObservation(localVelocity.x); // X-axis velocity
            sensor.AddObservation(localVelocity.z); // Z-axis velocity
        }
        else
        {
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
        }

        // Observe nearby traffic light state.
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, 5f, LayerMask.GetMask("TrafficLight"));
        if (nearbyColliders.Length > 0)
        {
            TrafficLightController trafficLight = nearbyColliders[0].GetComponent<TrafficLightController>();
            sensor.AddObservation(trafficLight != null && trafficLight.IsStopped ? 1f : 0f);
        }
        else
        {
            sensor.AddObservation(0f);
        }

        // Lane keeping observations.
        if (CurrentLaneId == "Outgoing")
        {
            Physics.Raycast(transform.position, -transform.right, out leftHit, rayLength, wallLayer);
            Physics.Raycast(transform.position, transform.right, out rightHit, rayLength, wallLayer);
            sensor.AddObservation(leftHit.distance / rayLength);
            sensor.AddObservation(rightHit.distance / rayLength);
        }
        else
        {
            sensor.AddObservation(1f);
            sensor.AddObservation(1f);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // --- Steering ---
        // Use the first continuous action as a steering delta.
        float steerDelta = Mathf.Clamp(actions.ContinuousActions[0], -maxSteeringDelta, maxSteeringDelta);
        // (Optional) Penalize large steering deltas to further encourage smooth steering.
        if (steeringDeltaPenaltyFactor > 0f)
        {
            AddReward(-Mathf.Abs(steerDelta) * steeringDeltaPenaltyFactor);
        }
        // Accumulate the delta into the current steering value.
        _currentSteer = Mathf.Clamp(_currentSteer + steerDelta, -1f, 1f);

        // --- Motor and Brake ---
        float targetMotor = Mathf.Clamp(actions.ContinuousActions[1], 0f, 1f);
        float targetBrake = Mathf.Clamp(actions.ContinuousActions[2], 0f, 1f);
        _currentMotor = Mathf.SmoothDamp(_currentMotor, targetMotor, ref _motorVelocity, motorSmoothTime);
        _currentBrake = Mathf.SmoothDamp(_currentBrake, targetBrake, ref _brakeVelocity, brakeSmoothTime);

        // Pass the inputs to the car controller.
        _carController.horizontalInput = _currentSteer;
        _carController.currentMotorInput = _currentMotor;
        _carController.isBraking = _currentBrake > 0.5f;

        // Add step penalty.
        AddReward(stepPenalty);

        // Apply idle penalty if nearly stopped (and not at a red light).
        if (_rb != null && _rb.linearVelocity.magnitude < 0.1f && !isAtRedLight)
        {
            AddReward(idlePenalty);
        }

        // Timeout: if the agent is near the maximum number of steps.
        if (StepCount >= MaxStep - 1 && MaxStep > 0)
        {
            AddReward(timeoutPenalty);
            _lastEpisodeReward = GetCumulativeReward();
            EndEpisode();
        }

        // Lane keeping penalty.
        if (CurrentLaneId == "Outgoing" && !isAtRedLight)
        {
            if (Physics.Raycast(transform.position, -transform.right, out leftHit, rayLength, wallLayer) &&
                Physics.Raycast(transform.position, transform.right, out rightHit, rayLength, wallLayer))
            {
                float centerOffset = Mathf.Abs(leftHit.distance - rightHit.distance);
                if (centerOffset > maxLateralDistance)
                {
                    AddReward(laneKeepingPenalty * centerOffset);
                }
            }
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        // For manual control, output a steering delta scaled by maxSteeringDelta.
        continuousActions[0] = Input.GetAxis("Horizontal") * maxSteeringDelta;
        continuousActions[1] = Input.GetAxis("Vertical");
        continuousActions[2] = Input.GetKey(KeyCode.Space) ? 1f : 0f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Collision with: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");
        
        if (collision.gameObject.CompareTag("Checkpoint"))
        {
            AddReward(targetReward);
            _lastEpisodeReward = GetCumulativeReward();
            EndEpisode();
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-50f);
            _lastEpisodeReward = GetCumulativeReward();
            EndEpisode();
            Debug.Log("Car hit a Wall! Penalty: -50");
        }
        else if (collision.gameObject.CompareTag("Truck"))
        {
            AddReward(-100f);
            _lastEpisodeReward = GetCumulativeReward();
            EndEpisode();
            Debug.Log("Car hit a Truck! Penalty: -100");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("TrafficLight"))
        {
            TrafficLightController trafficLight = other.GetComponent<TrafficLightController>();
            if (trafficLight != null)
            {
                if (trafficLight.IsStopped)
                {
                    if (_rb.linearVelocity.magnitude < 0.1f)
                    {
                        isAtRedLight = true;
                        AddReward(0.1f);
                    }
                }
                else
                {
                    if (_rb.linearVelocity.magnitude < 0.1f)
                    {
                        AddReward(-0.5f);
                        Debug.Log("Car is stopped at green light! -0.5 penalty");
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("TrafficLight"))
        {
            isAtRedLight = false;
            Debug.Log($"Exiting traffic light zone. isAtRedLight: {isAtRedLight}");
            
            TrafficLightController trafficLight = other.GetComponent<TrafficLightController>();
            if (trafficLight != null)
            {
                if (trafficLight.IsStopped)
                {
                    AddReward(-100f);
                    Debug.Log($"Car left traffic light zone during red light! -100 penalty. Reward: {GetCumulativeReward()}");
                }
                else
                {
                    AddReward(50f);
                    Debug.Log($"Car passed through green light! +50 reward. Reward: {GetCumulativeReward()}");
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Checkpoint"))
        {
            AddReward(10f);
            CurrentLaneId = "Outgoing"; // Reset lane to Outgoing.
            Debug.Log($"Car hit the checkpoint in lane {CurrentLaneId}! Reward: {GetCumulativeReward()}, Collider: {other.name}");
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("WrongCheckpoint"))
        {
            if (CurrentLaneId == "Outgoing")
            {
                Debug.Log("Correct lane detected! Switching to Incoming.");
                CurrentLaneId = "Incoming";
            }
            else
            {
                AddReward(-10f);
                Debug.Log($"Car hit wrong checkpoint in lane {CurrentLaneId}! Reward: {GetCumulativeReward()}, Collider: {other.name}");
                EndEpisode();
            }
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("TrafficLight"))
        {
            isAtRedLight = false;
            Debug.Log($"Entering traffic light zone. isAtRedLight: {isAtRedLight}");
            
            TrafficLightController trafficLight = other.GetComponent<TrafficLightController>();
            if (trafficLight != null)
            {
                if (trafficLight.IsStopped)
                {
                    if (_rb.linearVelocity.magnitude < 2f)
                    {
                        AddReward(10f);
                        Debug.Log($"Entering red light zone slowly! +10 reward. Reward: {GetCumulativeReward()}");
                    }
                }
                else
                {
                    AddReward(1f);
                    Debug.Log($"Proceeding through green light! +1 reward. Reward: {GetCumulativeReward()}");
                }
            }
        }
    }

    // Optional: Visualize sensor rays.
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, -transform.right * rayLength);
        Gizmos.DrawRay(transform.position, transform.right * rayLength);
    }

    private float _nextLogTime = 0f;
    private float _logInterval = 1f;

    private void Update()
    {
        if (Time.time >= _nextLogTime)
        {
            Debug.Log($"Current Total Reward: {GetCumulativeReward()}");
            _nextLogTime = Time.time + _logInterval;
        }

        if (CurrentLaneId == "Outgoing" &&
            Physics.Raycast(transform.position, -transform.right, out leftHit, rayLength, wallLayer) &&
            Physics.Raycast(transform.position, transform.right, out rightHit, rayLength, wallLayer))
        {
            float centerOffset = Mathf.Abs(leftHit.distance - rightHit.distance);
            string penaltyText = centerOffset > maxLateralDistance ?
                $"Lane Penalty: {laneKeepingPenalty * centerOffset:F4}" : "No Lane Penalty";
            if (isAtRedLight)
                penaltyText = "No Penalty (At Red Light)";
            
            Debug.Log($"Wall Distances - Left: {leftHit.distance:F2}m, Right: {rightHit.distance:F2}m | {penaltyText}");
        }
    }
}
