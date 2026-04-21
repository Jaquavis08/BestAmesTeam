using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Daycount : MonoBehaviour
{
    public static Daycount instance;

    public TMP_Text daycount;
    public int day = 0;
    public float time;
    public float daylengthBase = 10f; // minutes
    private float daylength; // minutes

    public Button NextDayButton;

    
    public void Awake()
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

    public void Start()
    {
        daycount.text = "Day: " + day;
        daylength = daylengthBase * 10f;
    }
    void Update()
    {
        ProccesTime();
    }

    void ProccesTime()
    {
        
        if (time >= daylength)
        {
            NPCSpawner.Instance.SpawningNPC = false;
            NextDayButton.interactable = true;
        }
        else
        {
            time += Time.deltaTime;
            NPCSpawner.Instance.SpawningNPC = true;
            NextDayButton.interactable = false;
        }
    }

    public void NextDay()
    {
        if(TaskDisplayer.instance.CheckForCompleteQuota())
        {
            time = 10f;
            day++;
            daycount.text = "Day: " + day;
            TaskDisplayer.instance.GetQuotaFormula();
            if (TaskDisplayer.instance.Tasks.Count > 7)
                TaskDisplayer.instance.Tasks[7].completed = true;
        }
        else
        {
            Death();
        }
    }

    void Death()
    {
        Console.Clear();
        SceneManager.LoadScene("MINE 1");
        print("You Died Had A Death");
    }
}
