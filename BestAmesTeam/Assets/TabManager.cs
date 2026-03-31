using System.Collections.Generic;
using UnityEngine;

public class TabManager : MonoBehaviour
{
    
    public List<GameObject> tabs = new List<GameObject>();
    public List<GameObject> tabButtons = new List<GameObject>();

    public void SelectTab(int tabNumber)
    {
            foreach (GameObject tab in tabs)
            {
                tab.SetActive(false);
            }
    
            if (tabNumber >= 0 && tabNumber < tabs.Count)
            {
                tabs[tabNumber].SetActive(true);
            }

        SelectTabButton(tabNumber);
    }

    public void SelectTabButton(int tabNumber)
    {
        foreach (GameObject button in tabButtons)
        {
            button.transform.GetChild(0).gameObject.SetActive(false);
        }
        if (tabNumber >= 0 && tabNumber < tabButtons.Count)
        {
            tabButtons[tabNumber].transform.GetChild(0).gameObject.SetActive(true);
        }
    }
}
