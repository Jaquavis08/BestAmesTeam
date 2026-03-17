using UnityEngine;

public class Placeholdercomputerui : MonoBehaviour
{
    public GameObject computerUI;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleComputerUI();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (computerUI != null && computerUI.activeSelf)
            {
                ToggleComputerUI();
            }
        }
    }

    private void ToggleComputerUI()
    {
        if (computerUI == null)
        {
            Debug.LogWarning("Placeholdercomputerui: computerUI reference is missing.");
            return;
        }

        Time.timeScale = computerUI.activeSelf ? 1f : 0f; // Pause game when UI is active
        Cursor.lockState = CursorLockMode.None;

        bool isActive = computerUI.activeSelf;
        computerUI.SetActive(!isActive);
    }
}
