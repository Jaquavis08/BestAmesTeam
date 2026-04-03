using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class CarEnterExit : MonoBehaviour
{
    [Header("References")]
    public GameObject player;
    public Car carController;
    public Rigidbody carRb;
    public VisualEffect exhaustParticles;
    public VisualEffect offroadparticles;
    public VisualEffect offroadparticles2;

    public bool isOffroad = false;

    // Track dirt colliders separately to avoid flipping isOffroad incorrectly.
    private readonly HashSet<Collider> _dirtColliders = new HashSet<Collider>();


    [Header("Exhaust")]
    [Tooltip("A GameObject you can attach exhaust visuals/particles to. Toggles on when entering the car.")]
    public GameObject exhaustHolder;

    [Header("Settings")]
    public KeyCode enterExitKey = KeyCode.E;
    public float interactionDistance = 3f;
    public float exitSpeedThreshold = 0.5f;

    [Header("camera")]
    public GameObject carCamera;
    public GameObject playerCamera;

    [Header("Exit Settings")]
    public float exitCheckRadius = 0.5f;
    public LayerMask obstacleLayers;
    public Transform exitPoint;
    public BoxCollider canExitCheck;
    public Transform CenterCheck;
    public LayerMask exitCheckLayer;
    public GameObject carPrefab;

    [Header("Box Exclusions (for Check BoxCollider)")]
    [Tooltip("Any collider on these layers will be ignored as blockers")]
    public LayerMask excludedLayers;
    [Tooltip("Any collider with any of these tags will be ignored")]
    public string[] excludedTags;
    [Tooltip("Specific GameObjects to ignore (colliders belonging to these will be ignored)")]
    public GameObject[] excludedObjects;
    private readonly HashSet<Collider> _blockingColliders = new HashSet<Collider>();

    private bool inCar = false;

    [Header("Move/Teleport Settings")]
    [Tooltip("Duration in seconds to smoothly move the object to the target instead of teleporting")]
    public float moveDuration = 1.0f;
    private Coroutine _moveCoroutine;



    private void Start()
    {
        // Ensure exhaust visuals reflect current inCar state at start
        if (exhaustHolder != null)
        {
            exhaustHolder.SetActive(inCar);
        }

        if (exhaustParticles != null)
        {
            if (inCar)
            {
                // Start exhaust VFX when in car
                exhaustParticles.Play();
            }
            else
            {
                // Stop exhaust VFX when not in car
                exhaustParticles.Stop();
            }
        }

        // Ensure offroad particle initial states are consistent
        if (offroadparticles != null && !isOffroad) offroadparticles.Stop();
        if (offroadparticles2 != null && !isOffroad) offroadparticles2.Stop();
    }

    void Update()
    {
        // Derive isOffroad from tracked dirt colliders to avoid toggling by unrelated colliders.
        isOffroad = _dirtColliders.Count > 0;

        // Safely play/stop offroad particle systems only when needed and when references exist.
        if (offroadparticles != null)
        {
            if (isOffroad)
            {
                offroadparticles.Play();
            }
            else
            {
                offroadparticles.Stop();
            }
        }

        if (offroadparticles2 != null)
        {
            if (isOffroad)
            {
                offroadparticles2.Play();
            }
            else
            {
                offroadparticles2.Stop();
            }
        }

        if (Input.GetKeyDown(enterExitKey))
        {
            
            if (!inCar)
            {
                EnterCar();
                return;
            }
            
            if (inCar)
            {
                Debug.Log("Exit key pressed while in car, attempting to exit...");
                TryExitCar();
            }
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (!inCar)
            {
                Vector3 target = new Vector3(CenterCheck.position.x, CenterCheck.position.y + 2f, CenterCheck.position.z);
                StartSmoothMoveTo(target, Quaternion.Euler(0, 0, 0));
            }
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (inCar)
            {
                transform.position = Vector3.zero;
                transform.position = new Vector3(CenterCheck.position.x + 410f, CenterCheck.position.y + 16f, CenterCheck.position.z + 440f);
                gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);

            }
        }
    }

    void EnterCar()
    {
        Debug.LogWarning("Entering car...");
        inCar = true;
        player.SetActive(false);
        carController.enabled = true;
        carRb.isKinematic = false;

        // Ensure any residual motion is left to physics while driving
        // (Do not force-zero velocities here; leave physics active)
        carCamera.SetActive(true);
        playerCamera.SetActive(false);

        // Turn on exhaust visuals / particles
        if (exhaustHolder != null)
        {
            exhaustHolder.SetActive(true);
        }

        if (exhaustParticles != null)
        {
            exhaustParticles.Play();
        }
    }
       
    void TryExitCar()
    {
        Debug.LogWarning("Attempting to exit car...");
        // Use linearVelocity instead of the obsolete velocity property
        Vector3 currentLinear = carRb.linearVelocity;
        if (currentLinear.magnitude < exitSpeedThreshold)
        {
            Vector3 pointExit = exitPoint.position;
            Debug.LogFormat("Exit point: {0} {1} {2}", pointExit.x, pointExit.y, pointExit.z);

            if (canExitCheck != null)
            {
                if (!IsExitBlocked())
                {
                    ExitCar(pointExit);
                }
                else
                {
                    Debug.Log("Cannot exit here, obstacle detected inside exit box!");
                }
            }
        }
        else
        {
            Debug.Log("Car is moving too fast to exit!");
        }
    }

    void ExitCar(Vector3 exitPoint)
    {
        Debug.Log("oogabooga");
        inCar = false;

        // Small upward offset to reduce clipping into the ground
        Vector3 spawnPosition = exitPoint + Vector3.up * 0.1f;

        // Place and enable the player
        player.transform.position = spawnPosition;
        player.SetActive(true);

        // Disable car control and make car static for the moment
        if (carController != null)
        {
            carController.enabled = false;
        }

        // Zero out velocities to prevent the car from moving unexpectedly after exit.
        // Use linearVelocity (and angularVelocity) as appropriate.
        carRb.linearVelocity = Vector3.zero;
        carRb.angularVelocity = Vector3.zero;

        // Optionally make the rigidbody kinematic so player isn't pushed immediately; mirrors EnterCar behavior.
        carRb.isKinematic = true;

        // Swap cameras
        if (carCamera != null) carCamera.SetActive(false);
        if (playerCamera != null) playerCamera.SetActive(true);

        // Turn off exhaust visuals / particles
        if (exhaustParticles != null)
        {
            exhaustParticles.Stop();
        }

        if (exhaustHolder != null)
        {
            exhaustHolder.SetActive(false);
        }
        print("Exit successful, player should now be at: " + spawnPosition);
    }

    private bool IsExcluded(Collider other)
    {
        if (other == null) return true;

        // Ignore trigger colliders by default
        if (other.isTrigger) return true;

        GameObject go = other.gameObject;

        // Excluded specific objects
        if (excludedObjects != null)
        {
            for (int i = 0; i < excludedObjects.Length; i++)
            {
                if (excludedObjects[i] == go) return true;
            }
        }

        if (excludedTags != null)
        {
            for (int i = 0; i < excludedTags.Length; i++)
            {
                string t = excludedTags[i];
                if (!string.IsNullOrEmpty(t) && go.CompareTag(t)) return true;
            }
        }

        if (excludedLayers != 0)
        {
            if (((1 << go.layer) & excludedLayers) != 0) return true;
        }

        return false;
    }


    // Public query to determine if exit is currently blocked by any non-excluded collider inside the Check box
    public bool IsExitBlocked()
    {
        
        // Ensure we have an exit point to raycast from.
        if (exitPoint == null)
        {
            Debug.LogWarning("IsExitBlocked: exitPoint is null - treating as blocked.");
            return true;
        }

        // Setup ray origin slightly above the exit point to avoid self-collision.
        Vector3 origin = exitPoint.position + Vector3.down * 0.1f;

        // Use exitCheckRadius as the ray distance; ensure a sensible minimum.
        float rayDistance = Mathf.Max(exitCheckRadius, 2.5f);

        // Perform the raycast downwards using the configured layer mask.
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hitInfo, rayDistance, exitCheckLayer))
        {
            Debug.LogFormat("Ground detected below exit point: hit '{0}' at distance {1:F2}. Exit not blocked.",
                hitInfo.collider != null ? hitInfo.collider.name : "unknown", hitInfo.distance);
            return false; // Not blocked (ground present)
        }
        else
        {
            Debug.Log("No ground detected below exit point within distance. Treating exit as blocked.");
            return true; // Blocked because there's no ground beneath the exit point
        }
    }

    // Trigger callbacks to maintain the blocking set.
    // Ensure the BoxCollider 'Check' on this GameObject has 'Is Trigger' = true and encompasses the exit area.

    public void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        // Track dirt colliders separately (even if they are trigger colliders)
        if (other.gameObject.CompareTag("dirt"))
        {
            _dirtColliders.Add(other);
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (IsExcluded(other)) return;

        if (!_blockingColliders.Contains(other))
        {
            _blockingColliders.Add(other);
        }

        // Keep dirt tracking while it stays in the trigger
        if (other.gameObject.CompareTag("dirt"))
        {
            if (!_dirtColliders.Contains(other))
            {
                _dirtColliders.Add(other);
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other == null) return;

        _blockingColliders.Remove(other);

        // Remove from dirt tracking if it leaves
        if (other.gameObject.CompareTag("dirt"))
        {
            _dirtColliders.Remove(other);
        }
    }

    // Starts a smooth move to the target position and rotation. Stops any existing move.
    private void StartSmoothMoveTo(Vector3 targetPos, Quaternion targetRot)
    {
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
            _moveCoroutine = null;
        }

        _moveCoroutine = StartCoroutine(MoveToPosition(targetPos, targetRot, moveDuration));
    }

    // Smoothly move and rotate the object to the target over duration seconds.
    private IEnumerator MoveToPosition(Vector3 targetPos, Quaternion targetRot, float duration)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        if (duration <= 0f)
        {
            transform.position = targetPos;
            transform.rotation = targetRot;
            _moveCoroutine = null;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;
        _moveCoroutine = null;
    }

    // Draw debug gizmos for the exit point raycast(s) and the check box.
    private void OnDrawGizmos()
    {
        // Draw nothing if we don't have an exit point.
        if (exitPoint == null) return;

        // Ray origin (match IsExitBlocked logic)
        Vector3 origin = exitPoint.position + Vector3.down * 0.1f;
        Vector3 dir = Vector3.down;

        // Ray distance (use same logic as IsExitBlocked, but keep a reasonable minimum for visualization)
        float rayDistance = Mathf.Max(exitCheckRadius, 2.5f);

        // Perform the same raycast used at runtime so the gizmo reflects actual hit result
        bool didHit = Physics.Raycast(origin, dir, out RaycastHit hitInfo, rayDistance, exitCheckLayer);

        // Ray color: green when it hits ground layer, red otherwise
        Gizmos.color = didHit ? Color.green : Color.red;
        Gizmos.DrawLine(origin, origin + dir * rayDistance);
        Gizmos.DrawWireSphere(origin + dir * rayDistance, 0.05f); // end marker
        Gizmos.DrawSphere(origin, 0.03f); // origin marker

        if (didHit)
        {
            // Mark hit point and draw a small normal line
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(hitInfo.point, 0.05f);
            Gizmos.DrawLine(hitInfo.point, hitInfo.point + hitInfo.normal * 0.25f);
        }

        // Also visualize the BoxCollider used to detect blockers (if assigned)
        if (canExitCheck != null)
        {
            Vector3 worldCenter = canExitCheck.transform.TransformPoint(canExitCheck.center);
            Vector3 worldSize = Vector3.Scale(canExitCheck.size, canExitCheck.transform.lossyScale);

            Color fill = new Color(0f, 1f, 1f, 0.12f);
            Gizmos.color = fill;
            Gizmos.DrawCube(worldCenter, worldSize);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(worldCenter, worldSize);
        }
    }
}
