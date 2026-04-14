using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RestockManager : MonoBehaviour
{
    public ItemDictionary itemDictionary;

    public List<RestockItem> restockItems;

    public Transform Parent;
    public GameObject RestockItem;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RestoreStock();
    }

    public void RestoreStock()
    {
        restockItems.Clear();
        for (int i = 0; i < itemDictionary.items.Length; i++)
        {
            print(itemDictionary.items[i].name);
            RestockItem restockItem = new RestockItem();

            restockItem.name = itemDictionary.items[i].name;
            restockItem.price = itemDictionary.items[i].price;
            restockItem.quantity = itemDictionary.items[i].quanity;
            restockItem.icon = itemDictionary.items[i].icon;


            restockItems.Add(restockItem);

            GameObject ItemRestockUI = Instantiate(RestockItem, Parent);

            ItemRestockUI.transform.GetChild(0).GetComponent<Image>().sprite = restockItem.icon;
            ItemRestockUI.transform.GetChild(1).GetComponent<TMP_Text>().text = restockItem.name;
            ItemRestockUI.transform.GetChild(2).GetComponent<TMP_Text>().text = "$" + restockItem.price.ToString();
            ItemRestockUI.transform.GetChild(3).GetComponent<TMP_Text>().text = "x" + restockItem.quantity.ToString();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
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
