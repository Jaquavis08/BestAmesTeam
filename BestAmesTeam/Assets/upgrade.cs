using UnityEngine;

public class upgrade : MonoBehaviour
{
    // set this in the Inspector to the component instance that exposes MaxStock
    public Shelf shelf;

    public ItemSpot spot1;
    public ItemSpot spot2;

    
    public float upgradeCost = 25;

    void Start()
    {
        shelf.GetComponentInChildren<ItemSpot>();
        
    }
    void Update()
    { 
        
        int UpgradeCost = Mathf.RoundToInt(upgradeCost);
      
    }
    public void Upgrade()
    {
        // charge the player (convert float cost to int)
        int costInt = Mathf.RoundToInt(upgradeCost);


        if (Currency.Instance.amount >= costInt)
        {
            Currency.Instance.amount -= costInt;
            upgradeCost *= 1.25f;
            
            if (shelf != null)
            {
            spot1.maxStock += 1;
            spot2.maxStock += 1;
            Debug.Log("Upgrade successful!");
            }
        }


       
    }
}

