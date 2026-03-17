using UnityEngine;

public class Computer : MonoBehaviour
{
    public static Computer instance;

    public float interactDistance = 3f;

    public GameObject computerUI;

    public void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            computerUI.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;

        }
    }

    public void UsePC()
    {
        computerUI.SetActive(true);
        print("Using computer");
        if (computerUI == null)
        {
            Debug.LogWarning("Placeholdercomputerui: computerUI reference is missing.");
            return;
        }

        //Time.timeScale = computerUI.activeSelf ? 1f : 0f; // Pause game when UI is active
        Cursor.lockState = CursorLockMode.None;

        //bool isActive = computerUI.activeSelf;
        //computerUI.SetActive(!isActive);
    }

}
