using TMPro;
using UnityEngine;

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
        print(daylength);
    }
    void Update()
    {
        time += Time.deltaTime;
        if (time >= daylength)
        {
            time = 0;
            day++;
            daycount.text = "Day: " + day;
        }
    }
}
