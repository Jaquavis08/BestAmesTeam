using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ItemBox : MonoBehaviour
{
    public ItemData itemType;
    public int itemCount = 10;
    public Animator animator;
    public KeyCode openKey = KeyCode.E;
    public Vector3 boxScale;

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

    private void Start()
    {
        boxScale = transform.localScale;
    }

    private void Update()
    {
        if (transform.localScale != boxScale)
        {
            transform.localScale = boxScale;
        }

        if (Input.GetKeyDown(openKey) || Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (PlayerPickup.Instance.heldBox == this && boxOpened != true)
            {
                boxOpened = true;
                print("Open key pressed");
                OpenBox();
                GetVisualItems();
            }
        }
        if (itemCount <= 0)
        {
            print("Box is empty");
            CloseBox();
        }

        if (PlayerPickup.Instance.heldBox == this)
        {
            PlayerPickup.Instance.ItemAndCountText.enabled = true;
            PlayerPickup.Instance.ItemAndCountText.text = $"{itemType.itemName}: x{itemCount}";
        }
        else if (PlayerPickup.Instance.heldBox == null)
        {
            PlayerPickup.Instance.ItemAndCountText.enabled = false;
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
        Vector3 position = new Vector3(0, -2f, 1);

        // Shelf adjustments
        if (itemType.itemName == "Shelf")
        {
            size = new Vector3(0.1f, 0.1f, 0.1f);
            position = new Vector3(0f, -1.5f, 1f);
        }
            GameObject instance = Object.Instantiate(prefab, parent);
        instance.transform.localPosition = position;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = size;
        instance.name = $"{prefab.name}";

        if (itemType.itemName == "Shelf")
        {
            foreach (MeshCollider child in instance.GetComponentsInChildren<MeshCollider>())
            {
                Debug.LogError("Found child: " + child.name);
                child.GetComponent<MeshCollider>().enabled = false;
            }
        }



        if (instance.GetComponent<NavMeshObstacle>() != null)
        {
            instance.GetComponent<NavMeshObstacle>().enabled = false;
            instance.GetComponent<BoxCollider>().enabled = false;
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
        boxOpened = false;
        if (animator != null)
        {
            animator.SetTrigger(CloseTriggerHash);
        }
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