using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Store/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public float price;
    public GameObject prefab;
    public Sprite icon;
}