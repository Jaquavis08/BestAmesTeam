using System.Collections;
using UnityEngine;

public class Computer : MonoBehaviour
{
    public static Computer instance;

    public bool IsComputerOn = false;

    public Transform PCCamPosition;
    public Transform PlayerCam;
    public GameObject Player;

    public GameObject computerUI;
    public GameObject computerOffScreen;

    // 🔥 ORIGINAL CAMERA STATE
    private Transform originalParent;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private Vector3 originalWorldPosition;
    private Quaternion originalWorldRotation;

    public float transitionSpeed = 6f;

    private bool isTransitioning = false;

    public void Awake()
    {
        if (instance != null && instance != this)
            Destroy(gameObject);
        else
            instance = this;
    }

    private void Update()
    {
        if (IsComputerOn && Input.GetKeyDown(KeyCode.Escape))
        {
            StartCoroutine(ExitPC());
        }
    }

    public void Button()
    {
        StartCoroutine(ExitPC());
    }

    public void UsePC(bool value)
    {
        TaskDisplayer.instance.Tasks[4].completed = true;

        if (isTransitioning) return;

        if (value)
            StartCoroutine(EnterPC());
        else
            StartCoroutine(ExitPC());
    }

    // =========================
    // 🔥 ENTER PC (SMOOTH)
    // =========================
    IEnumerator EnterPC()
    {

        isTransitioning = true;
        IsComputerOn = true;

        computerUI.SetActive(true);
        computerOffScreen.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        PlayerMovement.Instance.cursorLock = false;
        PlayerMovement.Instance.enabled = false;

        // 🔥 SAVE ORIGINAL (LOCAL + WORLD)
        originalParent = PlayerCam.parent;

        originalPosition = PlayerCam.localPosition;
        originalRotation = PlayerCam.localRotation;

        originalWorldPosition = PlayerCam.position;
        originalWorldRotation = PlayerCam.rotation;

        // 🔥 DETACH
        PlayerCam.SetParent(null);

        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * transitionSpeed;

            float smoothT = Mathf.SmoothStep(0, 1, t);

            PlayerCam.position = Vector3.Lerp(originalWorldPosition, PCCamPosition.position, smoothT);
            PlayerCam.rotation = Quaternion.Slerp(originalWorldRotation, PCCamPosition.rotation, smoothT);

            yield return null;
        }

        // 🔥 SNAP CLEANLY
        PlayerCam.SetParent(PCCamPosition);
        PlayerCam.localPosition = Vector3.zero;
        PlayerCam.localRotation = Quaternion.identity;

        // 🔥 HIDE PLAYER
        Player.SetActive(false);

        isTransitioning = false;
    }

    // =========================
    // 🔥 EXIT PC (SMOOTH)
    // =========================
    IEnumerator ExitPC()
    {
        isTransitioning = true;
        IsComputerOn = false;

        computerUI.SetActive(false);
        computerOffScreen.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 🔥 SHOW PLAYER AGAIN
        Player.SetActive(true);

        // 🔥 DETACH CAMERA
        PlayerCam.SetParent(null);

        Vector3 startPos = PlayerCam.position;
        Quaternion startRot = PlayerCam.rotation;

        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * transitionSpeed;

            float smoothT = Mathf.SmoothStep(0, 1, t);

            PlayerCam.position = Vector3.Lerp(startPos, originalWorldPosition, smoothT);
            PlayerCam.rotation = Quaternion.Slerp(startRot, originalWorldRotation, smoothT);

            yield return null;
        }

        // 🔥 RESTORE TO PLAYER (THIS FIXES YOUR 0,0,0 BUG)
        PlayerCam.SetParent(originalParent);
        PlayerCam.localPosition = originalPosition;
        PlayerCam.localRotation = originalRotation;

        PlayerMovement.Instance.enabled = true;
        PlayerMovement.Instance.cursorLock = true;

        isTransitioning = false;
    }
}