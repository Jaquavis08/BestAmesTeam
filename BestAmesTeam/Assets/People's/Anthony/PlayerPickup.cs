using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerPickup : MonoBehaviour
{
    public static PlayerPickup Instance;
    public Transform holdPoint;
    private float interactDistance = 4f;

    public TMP_Text ItemAndCountText;

    public ItemBox heldBox;

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.red);

        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Mouse0))
        {
            TryInteract(ray);
        }

        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Mouse1))
        {
            DropBox();
        }

        Check();

        //if (heldBox != null)
        //{

        //}
    }

    void TryInteract(Ray ray)
    {
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {

            Debug.Log("Ray hit: " + hit.collider.name);

            if (hit.collider.GetComponent<ItemBox>() && heldBox == null)
            {
                PickupBox(hit.collider.GetComponent<ItemBox>());
                return;
            }


            ItemSpot spot = hit.collider.GetComponentInParent<ItemSpot>();

            if (spot != null)
            {
                Debug.Log("Hit ItemSpot: " + spot.gameObject.name);
            }
            else
            {
                Debug.Log("No ItemSpot on object");
            }

            if (spot != null && heldBox != null)
            {
                print("Restocking spot: " + spot.name);
                spot.Restock(heldBox);

                if (heldBox.IsEmpty())
                {
                    StartCoroutine(DestroyAfterDelay(heldBox.gameObject, 1f));
                    heldBox = null;
                }
            }

            if (hit.collider.GetComponent<Computer>())
            {
                Computer.instance.UsePC(true);
                Debug.LogWarning("Interacted with computer");
            }
        }
        //else if()
        //{

        //}
    }

    public void Check()
    {
        if (heldBox == null || boxboi.instance.boxbox == null) return;

        Collider heldCollider = heldBox.GetComponent<Collider>();
        bool isInside = false;

        if (heldCollider != null)
        {
            Bounds boxBounds = boxboi.instance.boxbox.bounds;
            Bounds heldBounds = heldCollider.bounds;

            if (boxBounds.Contains(heldBounds.min) && boxBounds.Contains(heldBounds.max))
            {
                isInside = true;
            }
        }
        else
        {
            if (boxboi.instance.boxbox.bounds.Contains(heldBox.transform.position))
            {
                isInside = true;
            }
        }
    }

    void PickupBox(ItemBox box)
    {
        heldBox = box;

        box.transform.SetParent(holdPoint);
        box.transform.localPosition = Vector3.zero;
        box.transform.localRotation = Quaternion.identity;

        Rigidbody rb = box.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;

        box.GetComponent<Collider>().enabled = false;

        Debug.Log("Picked up box");
    }

    void DropBox()
    {
        if (heldBox == null) return;
        heldBox.transform.SetParent(null);
        Rigidbody rb = heldBox.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = false;
        heldBox.GetComponent<Collider>().enabled = true;
        if (heldBox.gameObject.activeSelf == false)
        {
            heldBox.gameObject.SetActive(true);
            print("Reactivated box");
        }
        heldBox.CloseBox();
        heldBox = null;
        Debug.Log("Dropped box");
    }

    private IEnumerator DestroyAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null)
        {
            Destroy(obj);
        }
    }
}