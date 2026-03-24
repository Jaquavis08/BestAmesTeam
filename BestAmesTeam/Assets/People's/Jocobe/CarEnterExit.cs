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

    private bool inCar = false;

    void Update()
    {
        if (Input.GetKeyDown(enterExitKey))
        {
            if (!inCar && Vector3.Distance(player.transform.position, transform.position) < interactionDistance)
            {
                EnterCar();
            }
            else
            {
                TryExitCar();
            }
        }
    }

    void EnterCar()
    {
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
        // Use linearVelocity instead of the obsolete velocity property
        Vector3 currentLinear = carRb.linearVelocity;
        if (currentLinear.magnitude < exitSpeedThreshold)
        {
            Vector3 exitPoint = transform.position + transform.right * 2f; // Exit to the right of the car
            if (!Physics.CheckSphere(exitPoint, exitCheckRadius, obstacleLayers))
            {
                ExitCar(exitPoint);
            }
            else
            {
                Debug.Log("Cannot exit here, obstacle detected!");
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
}
