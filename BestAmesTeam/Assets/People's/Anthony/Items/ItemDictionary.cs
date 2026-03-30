using UnityEngine;

[CreateAssetMenu(fileName = "ItemDictionary", menuName = "Scriptable Objects/ItemDictionary")]
public class ItemDictionary : ScriptableObject
{
    public ItemData[] items;
}
