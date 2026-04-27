using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RestockManager : MonoBehaviour
{
    public ItemDictionary itemDictionary;

    public List<RestockItem> restockItems;
    public List<RestockItem> FurnitureItems;

    public Transform ProductsParent;
    public Transform FurnitureParent;
    public GameObject RestockItem;


    void Start()
    {
        RestoreStock();
    }

    public void RestoreStock()
    {
        restockItems.Clear();

        foreach (var item in itemDictionary.items)
        {
            if (item.itemType == ItemType.Product)
            {
                RestockItem productItem = new()
                {
                    name = item.name,
                    price = item.price,
                    quantity = item.quanity,
                    icon = item.icon
                };

                restockItems.Add(productItem);

                GameObject ItemRestockUI = Instantiate(RestockItem, ProductsParent);

                ItemRestockUI.transform.GetChild(0).GetComponent<Image>().sprite = productItem.icon;
                ItemRestockUI.transform.GetChild(1).GetComponent<TMP_Text>().text = productItem.name;
                ItemRestockUI.transform.GetChild(2).GetComponent<TMP_Text>().text = "$" + productItem.price.ToString();
                ItemRestockUI.transform.GetChild(3).GetComponent<TMP_Text>().text = "x" + productItem.quantity.ToString();

                ItemRestockUI.GetComponent<Button>().onClick.AddListener(() => SpawnStock.Instance.SpawnManager(item));

                if (TaskDisplayer.instance.Tasks.Count > 2)
                    TaskDisplayer.instance.Tasks[2].completed = true;
                print(item.name);
            }
            else if (item.itemType == ItemType.Furniture)
            {
                RestockItem furnitureItem = new()
                {
                    name = item.name,
                    price = item.price,
                    quantity = item.quanity,
                    icon = item.icon
                };
                FurnitureItems.Add(furnitureItem);

                GameObject ItemFurnitureUI = Instantiate(RestockItem, FurnitureParent);

                ItemFurnitureUI.transform.GetChild(0).GetComponent<Image>().sprite = furnitureItem.icon;
                ItemFurnitureUI.transform.GetChild(1).GetComponent<TMP_Text>().text = furnitureItem.name;
                ItemFurnitureUI.transform.GetChild(2).GetComponent<TMP_Text>().text = "$" + furnitureItem.price.ToString();
                ItemFurnitureUI.transform.GetChild(3).GetComponent<TMP_Text>().text = "x" + furnitureItem.quantity.ToString();

                ItemFurnitureUI.GetComponent<Button>().onClick.AddListener(() => SpawnStock.Instance.SpawnManager(item));

                if (TaskDisplayer.instance.Tasks.Count > 2)
                    TaskDisplayer.instance.Tasks[2].completed = true;
                print(item.name);
            }
        }
    }


}

[System.Serializable]
public class RestockItem
{
    public string name;
    public int price;
    public int quantity;
    public Sprite icon;
}
