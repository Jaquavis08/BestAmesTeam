using UnityEngine;
using TMPro;

public class CheckoutItemUI : MonoBehaviour
{
    public TextMeshProUGUI text;

    public void Setup(string itemName, int quantity, float price)
    {
        text.text = itemName + " x" + quantity + " - $" + price.ToString("F2");
    }
}