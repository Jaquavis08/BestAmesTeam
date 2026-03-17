using UnityEngine;
using UnityEngine.UIElements;

//public class Computer : MonoBehaviour
//{
//    public float interactDistance = 3f;
//    ItemBox computer;

//    void Update()
//    {
//        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f));
//        Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.red);

//        if (Input.GetKeyDown(KeyCode.E))
//        {
//            TryInteract(ray);
//        }
//    }

    //void TryInteract(Ray ray)
    //{
    //    RaycastHit hit;

        //if (Physics.Raycast(ray, out hit, interactDistance))
        //{

        //    Debug.Log("Ray hit: " + hit.collider.name);

        //    if (hit.collider.GetComponent<ItemBox>() && computer == null)
        //    {
        //        PickupBox(hit.collider.GetComponent<ItemBox>());
        //        return;
        //    }
        //}
