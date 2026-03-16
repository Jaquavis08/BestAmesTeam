using UnityEngine;

public class ItemSpot : MonoBehaviour
{
    public ItemData item;
    public int itemCount = 4;
    public int maxStock = 10;

    public Transform standPoint;

    public bool occupied => item != null && itemCount > 0;

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

        return takenItem;
    }

    public void Restock(ItemBox box)
    {
        if (item == null)
        {
            item = box.itemType;
        }

        if (box.itemType != item) return;

        if (itemCount >= maxStock) return;

        if (box.TakeItem())
        {
            itemCount++;
            Debug.Log("Shelf restocked. Stock: " + itemCount);
        }
    }
}