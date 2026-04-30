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

    [SerializeField] private List<GameObject> allowedCollisionObjects = new List<GameObject>();

    void Update()
    {
        UpdateInput();

        if (_InPlacementMode && ShelfCheck())
        {
            if (PlayerPickup.Instance.heldBox.gameObject.activeSelf)
            {
                PlayerPickup.Instance.heldBox.gameObject.SetActive(false);
            }
            Debug.LogWarning(PlayerPickup.Instance.heldBox.enabled);

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
        Vector3 cameraForward = playerCamera.transform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        Vector3 rayOrigin = playerCamera.transform.position + cameraForward * DistanceFromPlayer;
        rayOrigin.y += 1.5f; // small lift above player

        Ray ray = new Ray(rayOrigin, Vector3.down);

        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, raycastDistance, placementlayer))
        {
            _currentPlacementposition = hitInfo.point;
        }
        else
        {
            // fallback: just place forward on ground level
            _currentPlacementposition = rayOrigin + Vector3.down * 2f;
        }

        Quaternion rotation = Quaternion.Euler(0f, playerCamera.transform.eulerAngles.y, 0f);

        if (_previewObj != null)
        {
            _previewObj.transform.position = _currentPlacementposition + Vector3.up * yOffset;
            _previewObj.transform.rotation = rotation;
        }
    }


    private void UpdateInput()
    {

        if (PlayerPickup.Instance.heldBox == null)
        {
            ExitPlacementMode();
        }

        if (!ShelfCheck())
            return;

        // ENTER MODE (key or mouse)
        if (!_InPlacementMode && (Input.GetKeyDown(enterExitKey) || Input.GetMouseButtonDown(0)))
        {
            EnterPlacementMode();
            return;
        }

        // EXIT MODE (ONLY key)
        if (_InPlacementMode && Input.GetKeyDown(enterExitKey))
        {
            ExitPlacementMode();
            return;
        }

        // PLACE OBJECT (mouse only)
        if (_InPlacementMode && Input.GetMouseButtonDown(0))
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
    public bool CanPlaceObject()
    {
        if(_previewObj == null)
            return false;

        // original validity plus ensure preview is not colliding with other colliders
        var validComp = _previewObj.GetComponent<validPlacement>();
        bool isValid = validComp != null ? validComp.IsValid : true;
        if (!isValid) return false;

        return !IsPreviewColliding();
    }

    // Returns true if any preview collider overlaps other non-preview colliders
    private bool IsPreviewColliding()
    {
        if (_previewObj == null)
            return false;

        Collider[] previewColliders = _previewObj.GetComponentsInChildren<Collider>();
        if (previewColliders == null || previewColliders.Length == 0)
            return false;

        foreach (var col in previewColliders)
        {
            if (!col.enabled)
                continue;

            // Handle BoxCollider
            var box = col as BoxCollider;
            if (box != null)
            {
                Vector3 worldCenter = col.transform.TransformPoint(box.center);
                Vector3 worldHalfExtents = Vector3.Scale(box.size * 0.5f, col.transform.lossyScale);
                Collider[] hits = Physics.OverlapBox(worldCenter, worldHalfExtents, col.transform.rotation, ~0, QueryTriggerInteraction.Ignore);
                foreach (var hit in hits)
                {
                    if (hit == null) continue;
                    if (IsPartOfPreview(hit)) continue;
                    if (IsAllowedCollision(hit)) continue;
                    return true;
                }
                continue;
            }

            // Handle SphereCollider
            var sph = col as SphereCollider;
            if (sph != null)
            {
                Vector3 worldCenter = col.transform.TransformPoint(sph.center);
                float maxScale = Mathf.Max(col.transform.lossyScale.x, Mathf.Max(col.transform.lossyScale.y, col.transform.lossyScale.z));
                float worldRadius = sph.radius * maxScale;
                Collider[] hits = Physics.OverlapSphere(worldCenter, worldRadius, ~0, QueryTriggerInteraction.Ignore);
                foreach (var hit in hits)
                {
                    if (hit == null) continue;
                    if (IsPartOfPreview(hit)) continue;
                    if (IsAllowedCollision(hit)) continue;
                    return true;
                }
                continue;
            }

            // Fallback: use bounds
            var bounds = col.bounds;
            Collider[] fallbackHits = Physics.OverlapBox(bounds.center, bounds.extents, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);
            foreach (var hit in fallbackHits)
            {
                if (hit == null) continue;
                if (IsPartOfPreview(hit)) continue;
                if (IsAllowedCollision(hit)) continue;
                return true;
            }
        }

        return false;
    }

    // Returns true if the collider belongs to an allowed object set in allowedCollisionObjects
    private bool IsAllowedCollision(Collider other)
    {
        if (other == null || allowedCollisionObjects == null || allowedCollisionObjects.Count == 0)
            return false;

        foreach (var allowed in allowedCollisionObjects)
        {
            if (allowed == null) continue;
            if (other.transform.IsChildOf(allowed.transform) || other.gameObject == allowed)
                return true;
        }

        return false;
    }

    private bool IsPartOfPreview(Collider other)
    {
        if (other == null || _previewObj == null) return false;
        return other.transform.IsChildOf(_previewObj.transform);
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

        //Destroy(placedObj.GetComponent<BoxCollider>());
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
        PlayerPickup.Instance.heldBox.gameObject.SetActive(true);
    }
}
