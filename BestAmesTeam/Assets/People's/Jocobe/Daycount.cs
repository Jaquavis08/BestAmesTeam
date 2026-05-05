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
    private float daylengthBase = 2.5f; // minutes
    private float daylength; // minutes

    public Button NextDayButton;

    public Transform SunTransform;
    public Vector3 sunMinRotation = new Vector3(0f, 0f, 0f);
    public Vector3 sunMaxRotation = new Vector3(-180f, 0f, 0f);

    public GameObject DeathUI;
    public bool DeathEnabled = false;

    public TMP_Text Clock;

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
        UpdateSunPosition(); 
    }
    void Update()
    {
        ProccesTime();
        UpdateClock();
    }

    void UpdateClock()
    {
        if (Clock == null) return;

        float normalizedTime = time / daylength;

        float totalMinutes = (normalizedTime * 12f * 60f) + (8 * 60);

        int hours = Mathf.FloorToInt(totalMinutes / 60f);
        int minutes = Mathf.FloorToInt(totalMinutes % 60f);


        minutes = Mathf.RoundToInt(minutes / 15f) * 15;

        if (minutes == 60)
        {
            minutes = 0;
            hours += 1;
        }

        // 🔥 CONVERT TO 12-HOUR FORMAT
        string ampm = hours >= 12 ? "PM" : "AM";

        int hours12 = hours % 12;
        if (hours12 == 0) hours12 = 12;

        Clock.text = string.Format("{0}:{1:00}{2}", hours12, minutes, ampm);
    }

    void ProccesTime()
    {
        if (time >= daylength)
        {
            NPCSpawner.Instance.SpawningNPC = false;
            NextDayButton.interactable = true;

            if (!TaskDisplayer.instance.CheckForCompleteQuota() && DeathEnabled == false)
            {
                DeathEnabled = true;
                Death();
            }
        }
        else
        {
            time += Time.deltaTime;
            NPCSpawner.Instance.SpawningNPC = true;
            NextDayButton.interactable = false;
        }

        UpdateSunPosition();
    }

    void UpdateSunPosition()
    {
        if (SunTransform == null)
            return;

        float t = 0f;
        if (daylength > 0f)
            t = Mathf.Clamp01(time / daylength);

        Quaternion minQ = Quaternion.Euler(sunMinRotation);
        Quaternion maxQ = Quaternion.Euler(sunMaxRotation);

        SunTransform.rotation = Quaternion.Lerp(minQ, maxQ, t);
    }

    public void NextDay()
    {
        if(TaskDisplayer.instance.CheckForCompleteQuota())
        {
            time = 0;
            day++;
            daycount.text = "Day: " + day;
            TaskDisplayer.instance.GetQuotaFormula();
            //if (TaskDisplayer.instance.Tasks.Count > 7)
                //TaskDisplayer.instance.Tasks[7].completed = true;

            UpdateSunPosition();
        }
        else
        {
            Death();
        }
    }

    void Death()
    {
        Console.Clear();
        DeathUI.SetActive(true);
        Time.timeScale = 0f;
        PlayerMovement.Instance.cursorLock = false;
        print("You Died Had A Death");
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
