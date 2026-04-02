using UnityEngine;

public class Wheel : MonoBehaviour
{
    public WheelCollider wheelCollider;
    public Transform wheelMesh;
    public bool wheelTurn;
    public bool wheelSuspensionFollow;

    void Update()
    {
        if (wheelCollider == null || wheelMesh == null)
        {
            return;
        }

        // Get current world position and rotation from the WheelCollider (includes suspension offset)
        Vector3 worldPos;
        Quaternion worldRot;
        wheelCollider.GetWorldPose(out worldPos, out worldRot);

        // Make the mesh follow the collider's suspension (up/down)
        if (wheelSuspensionFollow)
        {
            wheelMesh.position = worldPos;
        }

        // Apply steering yaw if requested. Preserve other local rotation axes.
        if (wheelTurn)
        {
            Vector3 localEuler = wheelMesh.localEulerAngles;
            // steerAngle is in degrees
            float steerYaw = wheelCollider.steerAngle;
            wheelMesh.localEulerAngles = new Vector3(localEuler.x, steerYaw, localEuler.z);
        }

        // Spin the wheel mesh according to wheel RPM
        float spinDegrees = wheelCollider.rpm / 60f * 360f * Time.deltaTime;
        wheelMesh.Rotate(spinDegrees, 0f, 0f, Space.Self);
    }
}
