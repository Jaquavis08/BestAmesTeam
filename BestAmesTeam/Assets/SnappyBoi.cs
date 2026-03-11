using System.Drawing;
using UnityEngine;

public class SnappyBoi : MonoBehaviour
{
    [ContextMenu("Snap")]

    public void SnapTree()
    {
        if (Physics.Raycast(transform.position + Vector3.up * 10, Vector3.down, out RaycastHit hit, 100))
        {
            transform.position = hit.point;
        }
    }
}