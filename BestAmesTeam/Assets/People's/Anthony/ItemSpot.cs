using UnityEngine;

public class ItemSpot : MonoBehaviour
{
    public ItemData item;
    public int itemCount = 4;
    public int maxStock = 10;

    public Transform standPoint;

    public bool occupied => item != null && itemCount > 0;

    void Start()
    {
        ValidateShelfItems();
    }

    private void Update()
    {
        if (transform.childCount - 1 < itemCount)
        {
            print("Shelf item count mismatch, validating...");
            ValidateShelfItems();
        }    
    }

    public ItemData TakeItem()
    {
        if (!occupied) return null;

        ItemData takenItem = item;

        itemCount--;

        if (itemCount <= 0)
        {
            item = null;
            itemCount = 0;
        }

        ValidateShelfItems();
        return takenItem;
    }

    public void Restock(ItemBox box)
    {
        if (item == null)
        {
            item = box.itemType;
        }

        ValidateShelfItems();

        if (box.itemType != item) return;

        if (itemCount >= maxStock) return;

        if (box.TakeItem())
        {
            itemCount++;
            Debug.Log("Shelf restocked. Stock: " + itemCount);
        }
    }

    public void ValidateShelfItems()
    {
        Transform parent = this.gameObject != null ? this.transform : transform;

        // Clear existing spawned items
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            GameObject child = parent.GetChild(i).gameObject;

            if(child.name != "StandPoint")
            {
                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
            }
        }

        if (item == null || itemCount <= 0) return;

        GameObject prefab = item.prefab;
        if (prefab == null) return;

        float spacing = 0.25f;
        float totalWidth = (itemCount - 1) * spacing;
        Vector3 startOffset = new Vector3(-totalWidth * 0.5f, 0f, 0f);

        for (int i = 0; i < itemCount; i++)
        {
            GameObject instance = Object.Instantiate(prefab, parent);
            instance.transform.localPosition = startOffset + new Vector3(i * spacing, 0f, 0f);
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = prefab.transform.localScale;
            instance.name = $"{prefab.name}_{i}";
        }
    }
}