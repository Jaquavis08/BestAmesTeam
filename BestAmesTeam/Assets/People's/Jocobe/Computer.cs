using UnityEngine;

public class Computer : MonoBehaviour
{
    public static Computer instance;

    public float interactDistance = 3f;
    public bool IsComputerOn = false;

    public GameObject computerUI;
    public GameObject computerOffScreen;

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
            UsePC(false);
        }
    }

    public void UsePC(bool value)
    {
        computerUI.SetActive(value);
        computerOffScreen.SetActive(!value);
        PlayerMovement.Instance.cursorLock = !value;

        IsComputerOn = value;
    }

}
