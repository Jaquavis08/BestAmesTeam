using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class TutorialPopUpManager : MonoBehaviour
{
    //ItemBox tutorials;
    //public float interactDistance = 3f;


    //void TryInteract(Ray ray)
    //{
    //    RaycastHit hit;

    //    if (Physics.Raycast(ray, out hit, interactDistance))
    //    {

    //        Debug.Log("Ray hit: " + hit.collider.name);

    //        if (hit.collider.GetComponent<ItemBox>() && tutorials == null)
    //        {
    //            PickupBox(hit.collider.GetComponent<ItemBox>());
    //            return;
    //        }
    //    }
    //}

    //void Tutorial(ItemBox box)
    //{
    //    box.transform.SetParent(holdPoint);
    //    box.transform.localPosition = Vector3.zero;
    //    box.transform.localRotation = Quaternion.identity;

    //    Rigidbody rb = box.GetComponent<Rigidbody>();
    //    if (rb) rb.isKinematic = true;

    //    box.GetComponent<Collider>().enabled = false;

    //    Debug.Log("Picked up box");
    //}

}