namespace Identity.Consumer.Models.Code;

public class TimeCode : IOneTimeCode
{
    public string? Value { get; set; }

    public DateTime Timestamp { get; set; }

    public TimeCode(DateTime timestamp)
    {
        Timestamp = timestamp;
    }
    
    public TimeCode(string value, DateTime timestamp)
    {
        Value = value;
        Timestamp = timestamp;
    }
}