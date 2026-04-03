using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Currency : MonoBehaviour
{
    public static Currency Instance;
    public int amount = 1000;
    public TextMeshProUGUI currencyText;
    

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }

    void Update()
    {
        if (currencyText == null)
           Debug.LogWarning("No TextMeshProUGUI component found on the Currency GameObject.");

        // Display formatted currency
        currencyText.text = $" $ {amount}";
    }

    public void AddCurrency(int value)
    {
        amount += value;
        TaskDisplayer.instance.currentQuotaMoneyCount += value;
        print(TaskDisplayer.instance.currentQuotaMoneyCount);
    }

    public void RemoveCurrency(int value)
    {
        amount -= value;
        TaskDisplayer.instance.currentQuotaMoneyCount -= value;
    }
}
