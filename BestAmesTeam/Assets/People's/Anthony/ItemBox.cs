using UnityEngine;

public class ItemBox : MonoBehaviour
{
    public ItemData itemType;
    public int itemCount = 10;

    public bool TakeItem()
    {
        if (itemCount <= 0) return false;

        itemCount--;
        return true;
    }

    public bool IsEmpty()
    {
        return itemCount <= 0;
    }
}