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
            GetVisualItems();
            //EnableShelfAccess();
        }
        if (itemCount <= 0)
        {
            CloseBox();
           // DisableShelfAccess();
        }
    }

    public void GetVisualItems()
    {
        Transform parent = this.gameObject != null ? this.transform : transform;

        // Clear existing spawned items
        for (int i = parent.childCount - 1; i >= 2; i--)
        {
            GameObject child = parent.GetChild(i).gameObject;

            if (child.name != "Armature.001")
            {
                if (child.name == "open") return;

                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
            }
        }

        if (itemType == null || itemCount <= 0) return;

        GameObject prefab = itemType.prefab;
        if (prefab == null) return;

        Vector3 size = prefab.transform.localScale;
        Vector3 position = new Vector3(0, -2.027f, 1);

        if (itemType.itemName == "Shelf")
        {
            size = new Vector3(size.x * 0.1f, size.y * 0.1f, size.z * 0.1f);
        }

        GameObject instance = Object.Instantiate(prefab, parent);
        instance.transform.localPosition = position;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = size;
        instance.name = $"{prefab.name}";
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