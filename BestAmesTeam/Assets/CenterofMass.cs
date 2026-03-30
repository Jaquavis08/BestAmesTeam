using UnityEngine;

public class CenterofMass : MonoBehaviour
{
    void OngizmosDraw()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) return;
       
        Vector3 comWorld = transform.TransformPoint(rb.centerOfMass);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(comWorld, 0.1f);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(comWorld, comWorld + Vector3.down * 0.5f);
    }
}
