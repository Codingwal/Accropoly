public struct WorldTime
{
    public float seconds;
    public int minutes;
    public int hours;
    public int day;
    public readonly float TimeOfDayInSeconds => hours * 3600 + minutes * 60 + seconds;

    private bool newHour;
    public readonly bool NewHour => newHour;
    public static float HoursToSeconds(int hours) { return hours * 3600; }
    public void Advance(float deltaTime, out bool newDay)
    {
        newHour = false;

        seconds += deltaTime;
        if (seconds >= 60)
        {
            seconds -= 60;
            minutes++;
            if (minutes >= 60)
            {
                minutes -= 60;
                hours++;
                newHour = true;
                if (hours >= 24)
                {
                    hours -= 24;
                    day++;
                    newDay = true;
                    return;
                }
            }
        }
        newDay = false;
    }
    public readonly override string ToString()
    {
        return $"{hours}:{minutes:00} ({day})"; // Example: "12:05 (13)"
    }
}