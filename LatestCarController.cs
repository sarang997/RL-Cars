using UnityEngine;

namespace Ezereal
{
    public class LatestCarController : MonoBehaviour
    {
        [Header("Wheel Colliders")]
        [SerializeField] private WheelCollider frontLeftWheelCollider;
        [SerializeField] private WheelCollider frontRightWheelCollider;
        [SerializeField] private WheelCollider rearLeftWheelCollider;
        [SerializeField] private WheelCollider rearRightWheelCollider;

        [Header("Wheel Meshes")]
        [SerializeField] private Transform frontLeftWheelMesh;
        [SerializeField] private Transform frontRightWheelMesh;
        [SerializeField] private Transform rearLeftWheelMesh;
        [SerializeField] private Transform rearRightWheelMesh;

        [Header("Car Settings")]
        [SerializeField] private float motorForce = 1000f;
        [SerializeField] private float brakeForce = 2000f;
        [SerializeField] private float maxSteerAngle = 30f;
        // [SerializeField] private float stopThreshold = 1f; // Speed below which car is considered stopped
        
        public Rigidbody rb;
        public float horizontalInput;
        public float currentMotorInput = 0f;
        public float brakeInput;
        public bool isBraking = false;
        
        private void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            // Get steering input
            horizontalInput = Input.GetAxis("Horizontal");
            
            // Get forward/reverse input
            float rawInput = Input.GetAxis("Vertical");
            
            // Get brake input
            isBraking = Input.GetKey(KeyCode.Space);
            brakeInput = isBraking ? 1f : 0f;

            // Handle motor input separately from braking
            if (!isBraking)
            {
                currentMotorInput = rawInput;
            }
            else
            {
                currentMotorInput = 0f; // Cut motor power while braking
            }
        }

        private void FixedUpdate()
        {
            // Apply steering
            float steerAngle = horizontalInput * maxSteerAngle;
            frontLeftWheelCollider.steerAngle = steerAngle;
            frontRightWheelCollider.steerAngle = steerAngle;

            // Get current speed
            float currentSpeed = rb.linearVelocity.magnitude;

            // Handle braking
            if (isBraking)
            {
                // Apply brakes to all wheels
                float brake = brakeForce;
                ApplyBrakesToAllWheels(brake);
                
                // Cut motor power while braking
                ApplyMotorToWheels(0f);
            }
            else
            {
                // Release brakes
                ApplyBrakesToAllWheels(0f);
                
                // Apply motor force
                float motorPower = currentMotorInput * motorForce;
                ApplyMotorToWheels(motorPower);
            }

            // Update wheel meshes
            UpdateWheelPose(frontLeftWheelCollider, frontLeftWheelMesh);
            UpdateWheelPose(frontRightWheelCollider, frontRightWheelMesh);
            UpdateWheelPose(rearLeftWheelCollider, rearLeftWheelMesh);
            UpdateWheelPose(rearRightWheelCollider, rearRightWheelMesh);
        }

        private void ApplyBrakesToAllWheels(float brakeForce)
        {
            frontLeftWheelCollider.brakeTorque = brakeForce;
            frontRightWheelCollider.brakeTorque = brakeForce;
            rearLeftWheelCollider.brakeTorque = brakeForce;
            rearRightWheelCollider.brakeTorque = brakeForce;
        }

        private void ApplyMotorToWheels(float motorPower)
        {
            rearLeftWheelCollider.motorTorque = motorPower;
            rearRightWheelCollider.motorTorque = motorPower;
        }

        private void UpdateWheelPose(WheelCollider collider, Transform mesh)
        {
            if (mesh == null) return;
            collider.GetWorldPose(out Vector3 position, out Quaternion rotation);
            mesh.SetPositionAndRotation(position, rotation);
        }
    }
}