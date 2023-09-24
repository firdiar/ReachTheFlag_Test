using System;
using Unity.Netcode;

[System.Serializable]
public struct SerializeableTime : INetworkSerializeByMemcpy
{
    public byte Hours;
    public byte Minutes;
    public byte Seconds;

    public SerializeableTime(byte hours, byte minutes, byte seconds)
    {
        this.Hours = hours;
        this.Minutes = minutes;
        this.Seconds = seconds;
    }

    public SerializeableTime(TimeSpan timeSpan)
    {
        this.Hours = (byte)timeSpan.Hours;
        this.Minutes = (byte)timeSpan.Minutes;
        this.Seconds = (byte)timeSpan.Seconds;
    }

    // Implicit conversion operator from DateTime to SerializeableTime
    public static implicit operator SerializeableTime(DateTime dateTime)
    {
        return new SerializeableTime(dateTime.TimeOfDay);
    }

    public override string ToString()
    {
        return $"{Hours:00}:{Minutes:00}:{Seconds:00}";
    }

    public TimeSpan ToTimeSpan()
    {
        return new TimeSpan(0, Hours, Minutes, Seconds);
    }

    public static SerializeableTime Now()
    {
        TimeSpan now = DateTime.UtcNow.TimeOfDay;
        return new SerializeableTime(now);
    }

    public DateTime ToDateTime()
    {
        return DateTime.UtcNow.Date.Add(ToTimeSpan());
    }
}
