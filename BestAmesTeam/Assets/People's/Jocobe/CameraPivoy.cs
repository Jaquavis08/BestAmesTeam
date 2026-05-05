using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CameraPivoy : MonoBehaviour
{
    [Header("Orbit Settings")]
    [SerializeField] private Transform centerTransform = null;
    [SerializeField] private float radius = 5f;
    [Tooltip("Degrees per second")]
    [SerializeField] private float angularSpeed = 45f;
    [SerializeField] private bool maintainHeight = true;
    [SerializeField] private float heightOffset = 0f;

    [Header("Path Sampling")]
    [SerializeField] private float sampleInterval = 0.1f;
    [SerializeField] private int maxPoints = 512;
    [SerializeField] private bool closeCircle = true;

    [Header("Line Renderer Defaults")]
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private Gradient lineColor = null;

    private LineRenderer lineRenderer;
    private readonly List<Vector3> points = new List<Vector3>(128);
    private Vector3 orbitCenter = Vector3.zero;
    private float angleDeg = 0f;
    private float sampleTimer = 0f;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }


        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = closeCircle;
        lineRenderer.widthMultiplier = lineWidth;
        if (lineColor != null)
        {
            lineRenderer.colorGradient = lineColor;
        }
        else
        {

            var g = new Gradient();
            g.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
            );
            lineRenderer.colorGradient = g;
        }


        orbitCenter = centerTransform != null ? centerTransform.position : transform.position;

        Vector3 offset = transform.position - orbitCenter;
        if (offset.sqrMagnitude > Mathf.Epsilon)
        {
            angleDeg = Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg;

            radius = offset.magnitude;
        }
        else
        {
            angleDeg = 0f;
        }


        points.Clear();
        points.Add(transform.position);
        
    }

    private void Update()
    {

        if (centerTransform != null)
        {
            orbitCenter = centerTransform.position;
        }


        angleDeg += angularSpeed * Time.deltaTime;
        if (angleDeg >= 360f || angleDeg <= -360f)
        {
            angleDeg %= 360f;
        }


        float rad = angleDeg * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * radius;
        Vector3 newPos = orbitCenter + offset;
        if (maintainHeight)
        {
            newPos.y = transform.position.y;
        }
        if (heightOffset != 0f)
        {
            newPos.y = orbitCenter.y + heightOffset;
        }

        transform.position = newPos;

     
        transform.LookAt(orbitCenter);

 
        sampleTimer += Time.deltaTime;
        if (sampleTimer >= sampleInterval)
        {
            sampleTimer = 0f;
            AddPoint(transform.position);
        }

 
        lineRenderer.loop = closeCircle;
    }

    private void AddPoint(Vector3 p)
    {
        points.Add(p);
        if (points.Count > maxPoints)
        {

            points.RemoveAt(0);
        }
        
    }

    

    public void SetCenter(Transform newCenter)
    {
        centerTransform = newCenter;
        orbitCenter = newCenter != null ? newCenter.position : orbitCenter;
    }

    private void OnDrawGizmos()
    {

        Gizmos.color = Color.yellow;
        Vector3 c = centerTransform != null ? centerTransform.position : (Application.isPlaying ? orbitCenter : transform.position);
        Gizmos.DrawSphere(c, 0.1f);

        const int segs = 64;
        Vector3 prev = c + new Vector3(Mathf.Cos(0f), 0f, Mathf.Sin(0f)) * radius;
        for (int i = 1; i <= segs; i++)
        {
            float a = (i / (float)segs) * Mathf.PI * 2f;
            Vector3 cur = c + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * radius;
            Gizmos.DrawLine(prev, cur);
            prev = cur;
        }
    }
}
