using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEditor.Build.Content;
using UnityEngine;

public class upgrade : MonoBehaviour
{
    // set this in the Inspector to the component instance that exposes MaxStock
    public Shelf shelf;

    public ItemSpot spot1;
    public ItemSpot spot2;

    void Start()
    {
        shelf.GetComponentInChildren<ItemSpot>();
    }

    public void Upgrade()
    {
        if (shelf != null)
        {
            spot1.maxStock += 1;
            spot2.maxStock += 1;
            Debug.Log("Upgrade successful!");
        }
    }
}

