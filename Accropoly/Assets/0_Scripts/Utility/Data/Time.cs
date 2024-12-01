public struct WorldTime
{
    public float seconds;
    public int minutes;
    public int hours;
    public int day;
    public void Advance(float deltaTime, out bool newDay)
    {
        seconds += deltaTime;
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
        return $"{hours}:{minutes} ({day})";
    }
}