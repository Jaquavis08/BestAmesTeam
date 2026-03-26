using System.Collections.Generic;
using UnityEngine;

public class CarEnterExit : MonoBehaviour
{
    [Header("References")]
    public GameObject player;
    public Car carController;
    public Rigidbody carRb;

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
    public BoxCollider Check;
    public Transform CenterCheck;
    public GameObject carPrefab;

    [Header("Box Exclusions (for Check BoxCollider)")]
    [Tooltip("Any collider on these layers will be ignored as blockers")]
    public LayerMask excludedLayers;
    [Tooltip("Any collider with any of these tags will be ignored")]
    public string[] excludedTags;
    [Tooltip("Specific GameObjects to ignore (colliders belonging to these will be ignored)")]
    public GameObject[] excludedObjects;

    // Internal tracking of blockers currently inside the Check trigger
    private readonly HashSet<Collider> _blockingColliders = new HashSet<Collider>();

    private bool inCar = false;

    void Update()
    {
        if (Input.GetKeyDown(enterExitKey))
        {
            Debug.LogWarning(Vector3.Distance(player.transform.position, CenterCheck.position) < interactionDistance);
            if (!inCar && Vector3.Distance(player.transform.position, CenterCheck.position) < interactionDistance)
            {
                EnterCar();
            }
            else if (inCar)
            {
                TryExitCar();
            }
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (inCar)
            {
                //transform.position = Vector3.zero;
                //transform.position = new Vector3(CenterCheck.position.x + 1f, CenterCheck.position.y, CenterCheck.position.z);
                //gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
                
            }
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        Instantiate(carPrefab, new Vector3(CenterCheck.position.x, CenterCheck.position.y +1f, CenterCheck.position.z), Quaternion.Euler(0, 0, 0));
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
    }

    void TryExitCar()
    {
        // Print all names of GameObjects currently tracked as blocking colliders
        PrintBlockingColliderNames();

        Debug.LogWarning("Attempting to exit car...");
        // Use linearVelocity instead of the obsolete velocity property
        Vector3 currentLinear = carRb.linearVelocity;
        if (currentLinear.magnitude < exitSpeedThreshold)
        {
            Vector3 pointExit = exitPoint.position;
            Debug.LogFormat("Exit point: {0} {1} {2}", pointExit.x, pointExit.y, pointExit.z);

            if (Check != null)
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
    }

    // Helper to determine whether a collider should be ignored (excluded) as a blocker
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

        // Excluded tags
        if (excludedTags != null)
        {
            for (int i = 0; i < excludedTags.Length; i++)
            {
                string t = excludedTags[i];
                if (!string.IsNullOrEmpty(t) && go.CompareTag(t)) return true;
            }
        }

        // Excluded layers
        if (excludedLayers != 0)
        {
            if (((1 << go.layer) & excludedLayers) != 0) return true;
        }

        return false; // not excluded -> counts as blocker
    }

    // Public query to determine if exit is currently blocked by any non-excluded collider inside the Check box
    public bool IsExitBlocked()
    {
        // Clean up any destroyed or null colliders
        _blockingColliders.RemoveWhere(c => c == null);
        return _blockingColliders.Count > 0;
    }

    // Trigger callbacks to maintain the blocking set.
    // Ensure the BoxCollider 'Check' on this GameObject has 'Is Trigger' = true and encompasses the exit area.
    public void OnTriggerEnter(Collider other)
    {
        if (IsExcluded(other)) return;

        _blockingColliders.Add(other);
    }

    public void OnTriggerStay(Collider other)
    {
        // Keep the collider tracked while it stays in the trigger if it's not excluded.
        if (IsExcluded(other)) return;

        if (!_blockingColliders.Contains(other))
        {
            _blockingColliders.Add(other);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other == null) return;
        _blockingColliders.Remove(other);
    }

    private void PrintBlockingColliderNames()
    {
        if (_blockingColliders == null || _blockingColliders.Count == 0)
        {
            Debug.Log("No blocking colliders.");
            return;
        }

        var names = new List<string>(capacity: _blockingColliders.Count);
        foreach (var col in _blockingColliders)
        {
            if (col == null) continue;
            var go = col.gameObject;
            names.Add(go != null ? go.name : "null");
        }

        if (names.Count == 0)
        {
            Debug.Log("No blocking colliders (all entries were null).");
        }
        else
        {
            Debug.Log("Blocking colliders: " + string.Join(", ", names));
        }
    }
}
