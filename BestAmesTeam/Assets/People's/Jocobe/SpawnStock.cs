using UnityEngine;

public class SpawnStock : MonoBehaviour
{
    public static SpawnStock Instance;
    private ItemDictionary itemDictionary = ShelfManager.Instance.ItemDictionary;
    public GameObject ItemStockPrefab;
    public Transform StockParent;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SpawnManager(ItemData item)
    {
        Debug.LogWarning("Spawning " + item);
        GameObject itemStock = Instantiate(ItemStockPrefab, StockParent.position, Quaternion.identity, StockParent);
        itemStock.GetComponent<ItemBox>().itemType = item;
        itemStock.GetComponent<ItemBox>().itemCount = item.quanity;
    }    


    
}
