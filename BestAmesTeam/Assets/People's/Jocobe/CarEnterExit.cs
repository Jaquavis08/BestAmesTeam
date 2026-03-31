using AYellowpaper.SerializedCollections.Editor.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEnterExit : MonoBehaviour
{
    [Header("References")]
    public GameObject player;
    public Car carController;
    public Rigidbody carRb;
    public ParticleSystem exhaustParticles;
    public ParticleSystem offroadparticles;
    public ParticleSystem offroadparticles2;

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
                if (!exhaustParticles.isPlaying) exhaustParticles.Play();
            }
            else
            {
                if (exhaustParticles.isPlaying) exhaustParticles.Stop();
            }
        }

        // Ensure offroad particle initial states are consistent
        if (offroadparticles != null && offroadparticles.isPlaying && !isOffroad) offroadparticles.Stop();
        if (offroadparticles2 != null && offroadparticles2.isPlaying && !isOffroad) offroadparticles2.Stop();
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
                if (!offroadparticles.isPlaying) offroadparticles.Play();
            }
            else
            {
                if (offroadparticles.isPlaying) offroadparticles.Stop();
            }
        }

        if (offroadparticles2 != null)
        {
            if (isOffroad)
            {
                if (!offroadparticles2.isPlaying) offroadparticles2.Play();
            }
            else
            {
                if (offroadparticles2.isPlaying) offroadparticles2.Stop();
            }
        }

        if (Input.GetKeyDown(enterExitKey))
        {
            
            if (!inCar)
            {
                EnterCar();
            }
            else
            {
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
            if (!exhaustParticles.isPlaying) exhaustParticles.Play();
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
            if (exhaustParticles.isPlaying) exhaustParticles.Stop();
        }

        if (exhaustHolder != null)
        {
            exhaustHolder.SetActive(false);
        }
    }


    // Public query to determine if exit is currently blocked by any non-excluded collider inside the Check box
    public bool IsExitBlocked()
    {
        Collider[] hits = Physics.OverlapBox(canExitCheck.center, canExitCheck.size / 2, Quaternion.identity, exitCheckLayer);

        return hits.Length == 0;
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
        if (other == null) return;

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
}
