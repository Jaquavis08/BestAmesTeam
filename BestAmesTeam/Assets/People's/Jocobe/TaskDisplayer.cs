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
    public float currentQuotaMoneyCount;
    public float currentQuotaForDay;

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
        var candidates = Tasks.Where(t => t.day == currentDay && t.completed == false).ToList();

        print($"Found {candidates.Count} candidate tasks for day {currentDay}");

        if (candidates.Count == 0)
        {
            TaskList.text = string.Empty;
            return;
        }

 
        int minOrder = candidates.Min(t => t.order);
        var selected = candidates.FirstOrDefault(t => t.order == minOrder);

        TaskList.text = selected != null ? selected.task : string.Empty;
    }

    void UpdateQuotaUI()
    {

        quotaTab.transform.GetChild(1).GetComponent<TMP_Text>().text =
            $"Day: {Daycount.instance.day}/5";

        quotaTab.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>().text =
            $"Quota: ${Mathf.Round(currentQuotaMoneyCount)}/${currentQuotaForDay}";

        if (currentQuotaForDay > 0)
        {
            quotaValue.GetComponent<Image>().fillAmount = (float)Mathf.Round(currentQuotaMoneyCount) / currentQuotaForDay;
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

        currentQuotaForDay = Mathf.RoundToInt(300 * (Daycount.instance.day + 1) / 1.5f);

        Debug.Log($"Quota: {currentQuotaForDay}");
    }
}

[System.Serializable]
public class Task
{
    [TextArea(3, 10)] public string task;
    public int day;
    public int order;
    public bool completed;
}
