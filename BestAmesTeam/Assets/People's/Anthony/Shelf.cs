using System.Collections.Generic;
using UnityEngine;

public class Shelf : MonoBehaviour
{
    public List<ItemSpot> spots = new List<ItemSpot>();

    void Awake()
    {
        spots.AddRange(GetComponentsInChildren<ItemSpot>());
    }

    void Start()
    {
        ShelfManager.Instance.shelves.Add(this);
    }

    public ItemSpot GetRandomSpotWithItem()
    {
        List<ItemSpot> available = new List<ItemSpot>();

        foreach (ItemSpot spot in spots)
        {
            Debug.Log("Checking spot: " + spot.name + " occupied: " + spot.occupied);

            if (spot.occupied)
            {
                available.Add(spot);
            }
        }

        Debug.Log("Available spots: " + available.Count);

        if (available.Count == 0)
            return null;

        return available[Random.Range(0, available.Count)];
    }

    public bool HasAnyItems()
    {
        foreach (ItemSpot spot in spots)
        {
            if (spot.occupied)
                return true;
        }
        return false;
    }
}