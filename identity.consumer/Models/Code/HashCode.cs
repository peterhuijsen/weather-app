namespace Identity.Consumer.Models.Code;

public class HashCode : IOneTimeCode
{
    public string? Value { get; set; }

    public long Counter { get; set; }

    public HashCode(string value, long counter)
    {
        Value = value;
        Counter = counter;
    }

    public HashCode(long counter)
    {
        Counter = counter;
    }
}