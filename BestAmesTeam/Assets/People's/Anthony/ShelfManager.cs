using System.Collections.Generic;
using UnityEngine;

public class ShelfManager : MonoBehaviour
{
    public static ShelfManager Instance;

    public List<Shelf> shelves = new List<Shelf>();

    void Awake()
    {
        Instance = this;
    }

    public Shelf GetRandomShelfWithItems()
    {
        List<Shelf> shelvesWithItems = new List<Shelf>();

        foreach (Shelf shelf in shelves)
        {
            if (shelf.HasAnyItems())
                shelvesWithItems.Add(shelf);
        }

        if (shelvesWithItems.Count == 0)
            return null;

        int index = Random.Range(0, shelvesWithItems.Count);
        return shelvesWithItems[index];
    }
}