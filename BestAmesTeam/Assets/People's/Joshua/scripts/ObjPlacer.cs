
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class ObjPlacer : MonoBehaviour
{
    [Header("Placement Parameters")]
    [SerializeField] private GameObject PlaceableObj;
    [SerializeField] private GameObject PreviewObj;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask placementlayer;

    [Header("Preveiw Material")]
    [SerializeField] private Material previewMaterial;
    [SerializeField] private Color validColor;
    [SerializeField] private Color invalidColor;

    [Header("Raycast Parameters")]
    [SerializeField] private float DistanceFromPlayer;
    [SerializeField] private float raycastStartVerticleOffset;
    [SerializeField] private float raycastDistance;

    public float yOffset = 1f;
    private GameObject _previewObj = null;
    private Vector3 _currentPlacementposition = Vector3.zero;
    public bool _InPlacementMode = false;
    [SerializeField] private bool _validPreviewState = false;

    [SerializeField] private List<NavMeshBuildSource> Sources;

    public Transform ShelfParent;

    public NavMeshSurface navMeshSurface;

    public KeyCode enterExitKey = KeyCode.P;

    void Update()
    {
        UpdateInput();

        if (_InPlacementMode && ShelfCheck())
        {
            if (PlayerPickup.Instance.heldBox.GetComponent<MeshRenderer>().enabled == true)
            {
                PlayerPickup.Instance.heldBox.GetComponent<MeshRenderer>().enabled = false;
            }

            UpdateCurrentPlacementPosition();

            print(CanPlaceObject());
            if (CanPlaceObject())
            {
                SetValidPreviewState();
            }
            else
            {
                SetInvalidPreviewState();
            }
        }
    }

    public bool ShelfCheck()
    {
        if (PlayerPickup.Instance.heldBox != null && PlayerPickup.Instance.heldBox.GetComponent<ItemBox>().itemType.itemName == "Shelf")
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void UpdateCurrentPlacementPosition()
    {
        Vector3 cameraFoward = new Vector3(playerCamera.transform.forward.x, 0f, playerCamera.transform.forward.z);
        cameraFoward.Normalize();

        Vector3 startPos = playerCamera.transform.position + (cameraFoward * DistanceFromPlayer);
        startPos.y += raycastStartVerticleOffset;

        RaycastHit hitInfo;
        if(Physics.Raycast(startPos, Vector3.down, out hitInfo, raycastDistance, placementlayer))
        {
            _currentPlacementposition = hitInfo.point;
        }

        Quaternion rotation = Quaternion.Euler(0f, playerCamera.transform.eulerAngles.y, 0f);
        _previewObj.transform.position = _currentPlacementposition += new Vector3(0f, yOffset, 0f);
        _previewObj.transform.rotation = rotation;
    }


    private void UpdateInput()
    {
        if (Input.GetKeyDown(enterExitKey) && ShelfCheck())
        {
            if (!_InPlacementMode)
            {
                EnterPlacementMode();
                

            }
            else if (_InPlacementMode)
            {
                ExitPlacementMode();
                
            }

        }
        else if (Input.GetMouseButtonDown(0) && _InPlacementMode)
        {
            PlaceObject();
        }
    }
    private void SetValidPreviewState()
    {
        previewMaterial.color = validColor;
        _validPreviewState = true;
    }
    private void SetInvalidPreviewState()
    {
        previewMaterial.color = invalidColor;
        _validPreviewState = false;
    }
    private bool CanPlaceObject()
    {
        if(_previewObj == null)
            return false;

        return _previewObj.GetComponent<validPlacement>().IsValid;
    }
    private void PlaceObject()
    {
        if (!_InPlacementMode || !_validPreviewState)
            return;
        Debug.Log("Placed object");
        Quaternion rotation = Quaternion.Euler(0f, playerCamera.transform.eulerAngles.y, 0f);
        GameObject placedObj = Instantiate(PlaceableObj, _currentPlacementposition, rotation, ShelfParent);

        Destroy(PlayerPickup.Instance.heldBox.gameObject);
        PlayerPickup.Instance.heldBox = null;

        Destroy(placedObj.GetComponent<BoxCollider>());
        ExitPlacementMode();
    }



    private void EnterPlacementMode()
    {
        Debug.Log("Entered placement mode");

        Quaternion rotation = Quaternion.Euler(0f, playerCamera.transform.eulerAngles.y, 0f);
        _previewObj = Instantiate(PreviewObj, _currentPlacementposition, rotation, transform);
        _InPlacementMode = true;
    }

    private void ExitPlacementMode()
    { 
        Debug.Log("Exit placement mode");
        Destroy( _previewObj );
        _previewObj = null;
        _InPlacementMode = false;
        
    }
}
