using UnityEngine;

public class Car : MonoBehaviour
{
    public Rigidbody rb;
    public WheelCollider wheel1, wheel2, wheel3, wheel4;
    public float drivespeed, steerspeed;
    public float brakeForce = 3000f;
    public float idleBrakeForce = 1000f; // Brake force applied when not accelerating

    float horizontalInput, verticalInput;

    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
    }

    private void FixedUpdate()
    {
        // Use stored input from Update to avoid mixing input reads across frames
        float motor = verticalInput * drivespeed;
        print(motor);

        bool isBraking = Input.GetKey(KeyCode.Space);

        if (isBraking)
        {
            // When braking, cut motor torque and apply brake torque
            motor = 0f;

            wheel1.brakeTorque = brakeForce;
            wheel2.brakeTorque = brakeForce;
            wheel3.brakeTorque = brakeForce;
            wheel4.brakeTorque = brakeForce;
        }
        else
        {
            // No braking: clear brake torque
            wheel1.brakeTorque = 0f;
            wheel2.brakeTorque = 0f;
            wheel3.brakeTorque = 0f;
            wheel4.brakeTorque = 0f;
        }

        if (verticalInput < 0.1f && verticalInput > -0.1f)
        {
            motor = 0f;

            wheel1.brakeTorque = idleBrakeForce;
            wheel2.brakeTorque = idleBrakeForce;
            wheel3.brakeTorque = idleBrakeForce;
            wheel4.brakeTorque = idleBrakeForce;
        }

        // Apply motor torque to wheels (will be zero while braking)
        wheel1.motorTorque = motor;
        wheel2.motorTorque = motor;
        wheel3.motorTorque = motor;
        wheel4.motorTorque = motor;

        // Apply steering
        wheel1.steerAngle = steerspeed * horizontalInput;
        wheel2.steerAngle = steerspeed * horizontalInput;
    }
    void OnDrawGizmos()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        Vector3 comWorld = (rb != null) ? transform.TransformPoint(rb.centerOfMass) : transform.position;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(comWorld, 0.1f);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(comWorld, comWorld + Vector3.down * 0.5f);
    }
}
