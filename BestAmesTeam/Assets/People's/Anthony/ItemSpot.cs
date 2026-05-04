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
            TaskDisplayer.instance.Tasks[3].completed = true;
        }
    }

    //public void ValidateShelfItems()
    //{
    //    Transform parent = this.gameObject != null ? this.transform : transform;

    //    // Clear existing spawned items
    //    for (int i = parent.childCount - 1; i >= 0; i--)
    //    {
    //        GameObject child = parent.GetChild(i).gameObject;

    //        if(child.name != "StandPoint")
    //        {
    //            if (Application.isPlaying)
    //            {
    //                Destroy(child);
    //            }
    //            else
    //            {
    //                DestroyImmediate(child);
    //            }
    //        }
    //    }

    //    if (item == null || itemCount <= 0) return;

    //    GameObject prefab = item.prefab;
    //    if (prefab == null) return;

    //    float spacing = item.objectDistance;
    //    float totalWidth = (itemCount - 1) * spacing;
    //    Vector3 startOffset = new Vector3(-totalWidth * 0.5f, 0f, 0f);

    //    for (int i = 0; i < itemCount; i++)
    //    {
    //        GameObject instance = Object.Instantiate(prefab, parent);
    //        instance.transform.localPosition = startOffset + new Vector3(i * spacing -1.35f, 0f, 0f);
    //        instance.transform.localRotation = Quaternion.identity;
    //        instance.transform.localScale = prefab.transform.localScale;
    //        instance.name = $"{prefab.name}_{i}";
    //    }
    //}

    public void ValidateShelfItems()
    {
        Transform parent = transform;

        // Clear existing spawned items
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            GameObject child = parent.GetChild(i).gameObject;

            if (child.name != "StandPoint")
            {
                if (Application.isPlaying)
                    Destroy(child);
                else
                    DestroyImmediate(child);
            }
        }

        if (item == null || itemCount <= 0) return;

        GameObject prefab = item.prefab;
        if (prefab == null) return;

        // ✅ GET BOX COLLIDER
        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null)
        {
            Debug.LogWarning("No BoxCollider found on ItemSpot");
            return;
        }

        // ✅ CENTER + SIZE
        Vector3 center = box.center;
        float width = box.size.x;

        float spacing = item.objectDistance;

        // Clamp max items to fit inside box
        int maxFit = Mathf.FloorToInt(width / spacing);
        int spawnCount = Mathf.Min(itemCount, maxFit);

        // ✅ START POSITION (centered)
        float totalWidth = (spawnCount - 1) * spacing;
        float startX = center.x - totalWidth / 2f;

        for (int i = 0; i < spawnCount; i++)
        {
            GameObject instance = Instantiate(prefab, parent);

            float xPos = startX + i * spacing;

            instance.transform.localPosition = new Vector3(
                xPos,
                center.y,
                center.z
            );

            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = prefab.transform.localScale;
            instance.name = $"{prefab.name}_{i}";
        }
    }
}