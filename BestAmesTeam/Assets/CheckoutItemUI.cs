using UnityEngine;
using TMPro;

public class CheckoutItemUI : MonoBehaviour
{
    public TextMeshProUGUI text;

    public void Setup(string itemName, int quantity, float price, float tax)
    {
        text.text = itemName + " x" + quantity + " - $" + price.ToString("F2") + " (Tax: $" + tax.ToString("F2") + ")";
    }
}