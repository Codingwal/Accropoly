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
public static class TimeManager
{
    private const float reallifeMinutesPerIngameDay = 10;
    private const float timeScale = 24 * 60 / reallifeMinutesPerIngameDay; // Calculate the timeScale at compile time

    public static event Action NextDay;
    public static DaysOfTheWeek currentDay;
    public static TimeOfTheDay timeOfTheDay;

    static TimeManager()
    {
        timeOfTheDay.NextDay += OnNextDay;
    }
    public static void StepTime(float timeStep)
    {
        timeOfTheDay.AdvanceTime(timeStep * timeScale);

    }
    private static void OnNextDay()
    {
        currentDay++;
        NextDay.Invoke();
    }
}
