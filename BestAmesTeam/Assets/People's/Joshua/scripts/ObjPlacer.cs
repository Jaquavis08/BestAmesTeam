using System.Runtime.CompilerServices;
using UnityEditor.PackageManager;
using UnityEngine;

public class ObjPlacer : MonoBehaviour
{
    [Header("Placement Parameters")]
    [SerializeField] private GameObject PlaceableObj;
    [SerializeField] private GameObject PreviewObj;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask placementlayer;

    [Header("preveiw Material")]
    [SerializeField] private Material previewMaterial;
    [SerializeField] private Material validColor;
    [SerializeField] private Material invalidColor;

    [SerializeField] private float DistanceFromPlayer;
    [SerializeField] private float raycastStartVerticleOffset;
    [SerializeField] private float raycastDistance;

    public float yOffset = 1f;

    private GameObject _previewObj = null;
    private Vector3 _currentPlacementposition = Vector3.zero;
    public bool _InPlacementMode = false;
    private bool _validPreviewState = false;

    public Transform ShelfParent;

    public KeyCode enterExitKey = KeyCode.P;

    void Update()
    {
        UpdateInput();

        if (_InPlacementMode)
        {
            UpdateCurrentPlacementPosition();
        }
            UpdateCurrentPlacementPosition();

            if(CanPlaceObject())
            {
                SetValidPreviewState();
            }
            else
            {
                SetInvalidPreviewState();
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
        if (Input.GetKeyDown(enterExitKey))
        {
           

            if (!_InPlacementMode && transform.childCount <= 0)
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

        previewMaterial.color = validColor.color;
        _validPreviewState = true;
    }
    private void SetInvalidPreviewState()
    {
        previewMaterial.color = invalidColor.color;
        _validPreviewState = false;
    }
    private bool CanPlaceObject()
    {
        if(_previewObj == null)
            return false;

        return _previewObj.GetComponentInChildren<validPlacement>().IsValid;
    }
    private void PlaceObject()
    {
        if (!_InPlacementMode || !_validPreviewState)
            return;
        Debug.Log("Placed object");
        Quaternion rotation = Quaternion.Euler(0f, playerCamera.transform.eulerAngles.y, 0f);
        GameObject placedObj = Instantiate(PlaceableObj, _currentPlacementposition, rotation, ShelfParent);
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
        _InPlacementMode =false;
        
    }
}
