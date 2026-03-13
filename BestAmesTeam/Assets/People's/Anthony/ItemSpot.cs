using UnityEngine;

public class ItemSpot : MonoBehaviour
{
    public ItemData item;
    public int itemCount = 4;

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
}