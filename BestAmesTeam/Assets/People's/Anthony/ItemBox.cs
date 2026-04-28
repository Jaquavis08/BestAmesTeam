using UnityEngine;
using UnityEngine.UI;

public class ItemBox : MonoBehaviour
{
    public ItemData itemType;
    public int itemCount = 10;
    public Animator animator;
    public KeyCode openKey = KeyCode.E;

    private static readonly int OpenTriggerHash = Animator.StringToHash("OpenTrigger");
    private static readonly int CloseTriggerHash = Animator.StringToHash("CloseTrigger");

    private bool shelfAccessEnabled = false;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    public void Start()
    {
        OpenBox();
        EnableShelfAccess();
        CloseBox();
        DisableShelfAccess();
    }

    private void Update()
    {
        if (Input.GetKeyDown(openKey) && !shelfAccessEnabled)
        {
            OpenBox();
            EnableShelfAccess();
        }
        if (itemCount <= 0)
        {
            CloseBox();
            DisableShelfAccess();
        }
    }

    public void OpenBox()
    {
        if (animator != null)
        {
            animator.SetTrigger(OpenTriggerHash);
        }
    }

    public void CloseBox()
    {
        if (animator != null)
        {
            animator.SetTrigger(CloseTriggerHash);
        }
    }



    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------


    private void DisableShelfAccess()
    {
        shelfAccessEnabled = false;
    }

    public bool IsShelfAccessible()
    {
        return shelfAccessEnabled;
    }

    private void EnableShelfAccess()
    {
        shelfAccessEnabled = true;
    }

    public bool TryPlaceItemOnShelf()
    {
        if (!shelfAccessEnabled) return false;

        itemCount--;
        return true;
    }
    public bool TakeItem()
    {
        if (itemCount <= 0) return false;

        itemCount--;
        return true;
    }

    public bool IsEmpty()
    {
        return itemCount <= 0;
    }
}