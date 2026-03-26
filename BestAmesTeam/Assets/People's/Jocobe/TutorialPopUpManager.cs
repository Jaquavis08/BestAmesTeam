using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class TutorialPopUpManager : MonoBehaviour
{
    public float interactDistance = 3f;
    public Transform HoldPoint;

    public GameObject boxUI;
    public GameObject RestockUI;

    private void Update()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.black);
        TryInteract(ray);
    }

    void TryInteract(Ray ray)
    {
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {

            //Debug.Log("Ray hit: " + hit.collider.name);

            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("HeldItem"))
            {
                boxUI.SetActive(true);
            }
            else
            {
                boxUI.SetActive(false);
            }

            if (hit.collider.gameObject.GetComponent<Shelf>() && HoldPoint.childCount > 0)
            {
                RestockUI.SetActive(true);
            }
            else
            {
                RestockUI.SetActive(false);
            }
        }
        else
        {
            DisableUI();
        }
    }

    void DisableUI()
    {
        boxUI.SetActive(false);
        RestockUI.SetActive(false);
    }

}