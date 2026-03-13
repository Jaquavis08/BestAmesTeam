using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerMovement : MonoBehaviour
{


    [SerializeField] private Transform playerCamera;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float jumpheight = 2f;
    [SerializeField] private float gravity = -9.8f;
 

    // Camera control settings
    [SerializeField] private float lookSensitivity = 100f;
    [SerializeField] private bool invertY = false;




    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector3 velocity;
    private bool isSprinting;
    public CapsuleCollider Collider;

    private bool isCrouching = false;

    // Look input state
    private Vector2 lookInput;
    private float cameraPitch = 0f;
    [SerializeField] private float mouseSmoothTime = 0.06f;
    private float targetCameraPitch = 0f;
    private float cameraPitchVelocity = 0f;

    private float currentYaw = 0f;
    private float targetYaw = 0f;
    private float yawVelocity = 0f;

    public bool canJump = false;

    void Start()
    {

        Collider = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("Rigidbody component missing on player.");
            enabled = false;
            return;
        }
        if (playerCamera != null)
        {
            cameraPitch = playerCamera.localEulerAngles.x;
            if (cameraPitch > 180f) cameraPitch -= 360f;
            targetCameraPitch = cameraPitch;
        }

        // initialize yaw tracking
        currentYaw = transform.eulerAngles.y;
        targetYaw = currentYaw;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (enabled == false)
            return;

        moveInput = context.ReadValue<Vector2>();
    }



    public void OnJump(InputAction.CallbackContext context)
    {
        if (enabled == false)
            return;

        if (rb == null) return;
        // simple ground check using collider/bounds
        bool grounded = false;
        if (Collider != null && canJump)
        {
            Vector3 origin = transform.position + Vector3.up * (Collider.height * 0.5f);
            grounded = Physics.CheckSphere(origin - Vector3.up * (Collider.height * 0.5f + 0.01f), 0.1f);
        }
        else
        {
            grounded = Physics.CheckSphere(transform.position + Vector3.down * 0.1f, 0.1f);
        }

        // only trigger jump on button press start to avoid repeated triggers
        if (context.started && grounded)
        {
            velocity.y = Mathf.Sqrt(jumpheight * -2f * gravity);
        }
    }
    public void OnLook(InputAction.CallbackContext context)
    {
        if (enabled == false)
            return;

        lookInput = context.ReadValue<Vector2>();
    }

    public void Update()
    {
        if (rb == null) return;

        // accumulate target pitch and yaw from look input
        targetYaw += lookInput.x * lookSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * lookSensitivity * Time.deltaTime * (invertY ? 1f : -1f);
        targetCameraPitch += mouseY;
        targetCameraPitch = Mathf.Clamp(targetCameraPitch, -89f, 89f);

        // smooth current yaw and pitch toward targets
        currentYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref yawVelocity, mouseSmoothTime);
        cameraPitch = Mathf.SmoothDamp(cameraPitch, targetCameraPitch, ref cameraPitchVelocity, mouseSmoothTime);

        // apply rotations
        if (playerCamera != null)
        {
            playerCamera.localEulerAngles = new Vector3(cameraPitch, 0f, 0f);
        }
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, currentYaw, transform.eulerAngles.z);


        // Build movement direction based on camera
        Vector3 forward = playerCamera != null ? playerCamera.forward : transform.forward;
        Vector3 right = playerCamera != null ? playerCamera.right : transform.right;
        forward.y = 0f; right.y = 0f;
        forward.Normalize(); right.Normalize();

        Vector3 moveDirection = forward * moveInput.y + right * moveInput.x;

        float usedSpeed = isSprinting ? sprintSpeed : speed;
        Vector3 horizontalVelocity = moveDirection * usedSpeed;

        // Ground check using capsule collider
        bool grounded = false;
        if (Collider != null)
        {
            Vector3 origin = transform.position + Vector3.up * (Collider.height * 0.5f);
            grounded = Physics.CheckSphere(origin - Vector3.up * (Collider.height * 0.5f + 0.01f), 0.1f);
        }
        else
        {
            grounded = Physics.CheckSphere(transform.position + Vector3.down * 0.1f, 0.1f);
        }

        // If on the ground and falling, snap to a small negative velocity to keep contact
        if (grounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        // apply gravity
        velocity.y += gravity * Time.deltaTime;

        Vector3 finalMotion = (horizontalVelocity + Vector3.up * velocity.y) * Time.deltaTime;

        // move using Rigidbody
        rb.MovePosition(rb.position + finalMotion);
    }
    public void OnCollisionStay(Collision other)
    {
        if (other.gameObject.CompareTag("floor"))
        {
            canJump = true;
        }
    }
    public void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("floor"))
        {
            canJump = false;
        }
    }
}