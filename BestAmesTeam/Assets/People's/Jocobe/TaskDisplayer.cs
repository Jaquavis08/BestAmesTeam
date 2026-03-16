using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class TaskDisplayer : MonoBehaviour
{
    public static TaskDisplayer instance;

    public TMP_Text TaskList;
    public List<Task> Tasks = new List<Task>();

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
}

[System.Serializable]
public class Task
{
    public string task;
    public int day;
    public int order;
    public bool completed;
}
