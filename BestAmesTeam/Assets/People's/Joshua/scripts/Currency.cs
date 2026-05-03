using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Currency : MonoBehaviour
{
    public static Currency Instance;
    public float amount = 0;
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
        currencyText.text = $" $ {FormatMoney(amount)}";
    }

    public string FormatMoney(float amount)
    {
        // 🔥 Handle thousands (k)
        if (amount >= 1000f)
        {
            float value = amount / 1000f;

            // Show 1 decimal ONLY if needed (e.g. 1.5k)
            if (value % 1 == 0)
                return value.ToString("0") + "k";
            else
                return value.ToString("0.0") + "k";
        }

        // 🔥 Normal money (always 2 decimal places)
        return amount.ToString("0.00");
    }

    public void AddCurrency(float value)
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
