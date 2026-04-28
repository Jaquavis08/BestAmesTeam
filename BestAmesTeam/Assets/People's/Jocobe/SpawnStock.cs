using UnityEditor.Rendering;
using UnityEngine;

public class SpawnStock : MonoBehaviour
{
    public static SpawnStock Instance;
    private ItemDictionary itemDictionary;
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

    private void Start()
    {
        itemDictionary = ShelfManager.Instance.ItemDictionary;
    }

    public void SpawnManager(ItemData item)
    {

        if (Currency.Instance.amount >= item.price)
        {
            Debug.LogWarning("Spawning " + item);
        Currency.Instance.amount -= item.price;
        GameObject itemStock = Instantiate(ItemStockPrefab, StockParent.position, Quaternion.identity, StockParent);
        itemStock.GetComponent<ItemBox>().itemType = item;
        itemStock.GetComponent<ItemBox>().itemCount = item.quanity;
        if (TaskDisplayer.instance.Tasks.Count > 4)
            TaskDisplayer.instance.Tasks[4].completed = true;

        }
        else
        {
           
            Debug.LogWarning("Not enough money to spawn " + item);
            return;
        }
       
    }    


    
}
