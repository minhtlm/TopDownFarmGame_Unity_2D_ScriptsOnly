using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [SerializeField] private GlobalLightController globalLightController;
    [SerializeField] private float incrementInterval = 7f;
    [SerializeField] private int minuteIncrement = 10;

    [Range(0,1)] private float timeofDay = 0f; // 0 = midnight, 1 = next midnight
    public float TimeOfDay => timeofDay;

    private float secondsPerDay;
    private float timer = 0f;
    private int minute = 0;
    private int hour = 0;

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

    private void Start()
    {
        secondsPerDay = (incrementInterval / minuteIncrement) * 60f * 24f;
    }

    private void Update()
    {
        // Update the time of day scale
        timeofDay += Time.deltaTime / secondsPerDay;
        if (timeofDay >= 1f)
        {
            timeofDay = 0f;
        }

        // Update the timer and check if it's time to increment the minute
        timer += Time.deltaTime;
        if (timer >= incrementInterval)
        {
            timer = 0f;
            IncreaseTimeByMinute(minuteIncrement);
        }
    }

    private void IncreaseTimeByMinute(int minutes)
    {
        minute += minutes;

        if (minute >= 60)
        {
            hour ++;
            minute -= 60;
            if (hour >= 24)
            {
                hour = 0;
            }
        }

        globalLightController.UpdateLightByHour(hour);
    }

    public string GetFormattedTime()
    {
        int displayHour = hour % 12;
        if (displayHour == 0) displayHour = 12;
        string amPm = hour < 12 ? "am" : "pm";
        return $"{displayHour}:{minute:D2} {amPm}";
    }
}
