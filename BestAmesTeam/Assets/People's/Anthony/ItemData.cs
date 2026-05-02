using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Store/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public int price;
    public GameObject prefab;
    public Texture Icon;
    public int quanity;
    public float objectDistance;
    public ItemType itemType;
}

public enum ItemType
{
    Product,
    Furniture,
}