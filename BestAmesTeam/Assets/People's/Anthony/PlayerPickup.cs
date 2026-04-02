using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    public Transform holdPoint;
    public float interactDistance = 3f;

    ItemBox heldBox;

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
                    Destroy(heldBox.gameObject);
                    heldBox = null;
                }
            }

            if (hit.collider.GetComponent<Computer>())
            {
                Computer.instance.UsePC();
            }
        }
        else if()
        {

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
        heldBox = null;
        Debug.Log("Dropped box");
    }
}