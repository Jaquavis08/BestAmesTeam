using System.Collections.Generic;
using UnityEngine;

public class validPlacement : MonoBehaviour
{
    [SerializeField] private LayerMask invalidLayers;

    public bool IsValid { get; private set; } = true;
    [SerializeField] private List<Collider> _collidingObjects = new List<Collider>();

    private void OnTriggerEnter(Collider other)
    {
        print("triggered");
        if (((1 << other.gameObject.layer) & invalidLayers) != 0)
        {
            _collidingObjects.Add(other);
            IsValid = false;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        print("exit");
        if (((1 << other.gameObject.layer) & invalidLayers) != 0)
        {
            _collidingObjects.Remove(other);
            IsValid = _collidingObjects.Count <= 0;
        }
    }
}
