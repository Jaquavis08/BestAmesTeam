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


    void Start()
    {
        RestoreStock();
    }

    public void RestoreStock()
    {
        restockItems.Clear();

        foreach (var item in itemDictionary.items)
        {
            RestockItem restockItem = new()
            {
                name = item.name,
                price = item.price,
                quantity = item.quanity,
                icon = item.icon
            };

            restockItems.Add(restockItem);

            GameObject ItemRestockUI = Instantiate(RestockItem, Parent);

            ItemRestockUI.transform.GetChild(0).GetComponent<Image>().sprite = restockItem.icon;
            ItemRestockUI.transform.GetChild(1).GetComponent<TMP_Text>().text = restockItem.name;
            ItemRestockUI.transform.GetChild(2).GetComponent<TMP_Text>().text = "$" + restockItem.price.ToString();
            ItemRestockUI.transform.GetChild(3).GetComponent<TMP_Text>().text = "x" + restockItem.quantity.ToString();

            ItemRestockUI.GetComponent<Button>().onClick.AddListener(() => SpawnStock.Instance.SpawnManager(item));
           
            if (TaskDisplayer.instance.Tasks.Count > 2)
                TaskDisplayer.instance.Tasks[2].completed = true;
            print(item.name);
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
