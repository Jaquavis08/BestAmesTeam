using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Currency : MonoBehaviour
{
    public int amount = 100;
    public TextMeshProUGUI currencyText;
    

    void Start()
    {
       currencyText = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (currencyText == null)
           currencyText = GetComponent<TextMeshProUGUI>();
        

       


        // Display formatted currency
        currencyText.text = $" $ {amount}";
       
    }
}
