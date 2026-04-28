using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ItemBox : MonoBehaviour
{
    public ItemData itemType;
    public int itemCount = 10;
    public Animator animator;
    public KeyCode openKey = KeyCode.E;

    private static readonly int OpenTriggerHash = Animator.StringToHash("OpenTrigger");
    private static readonly int CloseTriggerHash = Animator.StringToHash("CloseTrigger");

    public bool boxOpened = false;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    public void Start()
    {
        //OpenBox();
        //EnableShelfAccess();
        CloseBox();
        //DisableShelfAccess();
    }

    private void Update()
    {
        if (Input.GetKeyDown(openKey))
        {
            OpenBox();
            //EnableShelfAccess();
        }
        if (itemCount <= 0)
        {
            CloseBox();
           // DisableShelfAccess();
        }
    }

    public void OpenBox()
    {
        if (animator != null)
        {
            animator.SetTrigger(OpenTriggerHash);
        }
        boxOpened = true;
    }

    public void CloseBox()
    {
        if (animator != null)
        {
            animator.SetTrigger(CloseTriggerHash);
        }
        boxOpened = false;
    }



    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------


    //private void DisableShelfAccess()
    //{
    //    shelfAccessEnabled = false;
    //}

    //public bool IsShelfAccessible()
    //{
    //    return shelfAccessEnabled;
    //}

    //private void EnableShelfAccess()
    //{
    //    shelfAccessEnabled = true;
    //}

    //public bool TryPlaceItemOnShelf()
    //{
    //    if (!boxOpened) return false;
    //    if (itemCount <= 0) return false;

    //    itemCount = UnityEngine.Mathf.Max(0, itemCount - 1);
    //    return true;
    //}
    public bool TakeItem()
    {
        if (itemCount <= 0) return false;

        if (!boxOpened) return false;

        itemCount--;
        return true;
    }

    public bool IsEmpty()
    {
        return itemCount <= 0;
    }
}