using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Daycount : MonoBehaviour
{
    public static Daycount instance;

    public TMP_Text daycount;
    public int day = 0;
    public float time;
    public float daylengthBase = 10f; // minutes
    private float daylength; // minutes

    
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
        daylength = daylengthBase * 60f;
    }
    void Update()
    {
        ProccesTime();
    }

    void ProccesTime()
    {
        time += Time.deltaTime;
        if (time >= daylength)
        {
            if(TaskDisplayer.instance.CheckForCompleteQuota())
            {
                time = 0;
                day++;
                daycount.text = "Day: " + day;
                TaskDisplayer.instance.GetQuotaFormula();
            }
            else
            {
                Death();
            }
        }
    }

    void Death()
    {
        Console.Clear();
        SceneManager.LoadScene("MINE 1");
        print("You Died Had A Death");
    }
}
