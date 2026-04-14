using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Store/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public int price;
    public GameObject prefab;
    public Sprite icon;
    public int quanity;
}