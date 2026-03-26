using System.Collections.Generic;
using UnityEngine;

public class ShelfManager : MonoBehaviour
{
    public static ShelfManager Instance;

    public GameObject BoxPrefab;
    public ItemDictionary ItemDictionary;

    public Transform shelvesParent;

    public List<Shelf> shelves = new List<Shelf>();

    void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        shelves.AddRange(shelvesParent.GetComponentsInChildren<Shelf>());
    }

    public Shelf GetRandomShelfWithItems()
    {
        List<Shelf> shelvesWithItems = new List<Shelf>();

        foreach (Shelf shelf in shelves)
        {
            if (shelf.HasAnyItems())
            {
                print("Shelf has items");
                shelvesWithItems.Add(shelf);
            }
        }

        if (shelvesWithItems.Count == 0)
            return null;

        int index = Random.Range(0, shelvesWithItems.Count);
        return shelvesWithItems[index];
    }
}