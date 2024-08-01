using System;
using UnityEngine;

public enum DaysOfTheWeek
{
    Monday,
    Tuesday,
    Wednesday,
    Thursday,
    Friday,
    Saturday,
    Sunday,
}
public struct TimeOfTheDay
{
    public event Action NextDay;
    public int hours;
    public int minutes;
    public float seconds;
    public void AdvanceTime(float _seconds)
    {
        seconds = _seconds;
        if (seconds >= 60)
        {
            seconds -= 60;
            minutes++;

            if (minutes >= 60)
            {
                minutes -= 60;
                hours++;

                if (hours >= 24)
                {
                    hours -= 24;
                    NextDay.Invoke();
                }
            }
        }
    }
}
public class TimeManager : MonoBehaviour
{
    private const float reallifeMinutesPerIngameDay = 10;
    private const float timeScale = 24 * 60 / reallifeMinutesPerIngameDay; // Calculate the timeScale at compile time

    public event Action NextDay;
    public DaysOfTheWeek currentDay;
    public TimeOfTheDay timeOfTheDay;

    private void OnEnable()
    {
        timeOfTheDay.NextDay += OnNextDay;
    }
    private void OnDisable()
    {
        timeOfTheDay.NextDay -= OnNextDay;
    }
    private void FixedUpdate()
    {
        if (GameLoopManager.Instance.GameState != GameState.InGame) return;

        timeOfTheDay.AdvanceTime(Time.fixedDeltaTime * timeScale);

    }
    private void OnNextDay()
    {
        currentDay++;
        NextDay.Invoke();
    }
}
