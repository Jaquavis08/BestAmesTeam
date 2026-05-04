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

        Vector3 worldPos;
        Quaternion worldRot;
        wheelCollider.GetWorldPose(out worldPos, out worldRot);

        if (wheelSuspensionFollow)
        {
            wheelMesh.position = worldPos;
        }

        if (wheelTurn)
        {
            Vector3 localEuler = wheelMesh.localEulerAngles;
            float steerYaw = wheelCollider.steerAngle;
            wheelMesh.localEulerAngles = new Vector3(localEuler.x, steerYaw, localEuler.z);
        }

        float spinDegrees = wheelCollider.rpm / 60f * 360f * Time.deltaTime;
        wheelMesh.Rotate(spinDegrees, 0f, 0f, Space.Self);
    }
}
