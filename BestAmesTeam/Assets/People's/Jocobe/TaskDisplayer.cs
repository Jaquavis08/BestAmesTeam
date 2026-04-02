using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskDisplayer : MonoBehaviour
{
    public static TaskDisplayer instance;

    public TMP_Text TaskList;
    public List<Task> Tasks = new List<Task>();

    public GameObject MainPC;

    public GameObject quotaTab;
    public int currentQuotaMoneyCount;
    public int currentQuotaForDay;

    public GameObject quotaValue;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Daycount.instance == null || TaskList == null)
        {
            return;
        }

        if (quotaValue == null)
        {
            quotaValue = quotaTab.transform.GetChild(0).transform.GetChild(0).gameObject;
        }
        else
        {
            if (MainPC.activeSelf == true)
                UpdateQuotaUI();
        }


        int currentDay = Daycount.instance.day;
        // Filter tasks for current day that are not completed
        var candidates = Tasks.Where(t => t.day == currentDay && t.completed == false).ToList();

        if (candidates.Count == 0)
        {
            TaskList.text = string.Empty;
            return;
        }

        // Find the lowest order among candidates and pick the first matching task
        int minOrder = candidates.Min(t => t.order);
        var selected = candidates.FirstOrDefault(t => t.order == minOrder);

        TaskList.text = selected != null ? selected.task : string.Empty;
    }

    void UpdateQuotaUI()
    {

        quotaTab.transform.GetChild(2).GetComponent<TMP_Text>().text =
            $"Day: {Daycount.instance.day}/5";

        quotaTab.transform.GetChild(1).GetComponent<TMP_Text>().text =
            $"Quota: ${currentQuotaMoneyCount}/${currentQuotaForDay}";

        if (currentQuotaForDay > 0)
        {
            quotaValue.GetComponent<Image>().fillAmount = (float)currentQuotaMoneyCount / currentQuotaForDay;
        }
        else
        {
            quotaValue.GetComponent<Image>().fillAmount = 0f;
        }
        print($"Updated Quota UI");
    }

    public bool CheckForCompleteQuota()
    {
         return currentQuotaMoneyCount >= currentQuotaForDay;
    }

    public void GetQuotaFormula()
    {
        currentQuotaMoneyCount = 0;
        // FIXED formula (uses float properly)
        currentQuotaForDay = Mathf.RoundToInt(300 * (Daycount.instance.day + 1) / 1.5f);

        Debug.Log($"Quota: {currentQuotaForDay}");
    }
}

[System.Serializable]
public class Task
{
    public string task;
    public int day;
    public int order;
    public bool completed;
}
