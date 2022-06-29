namespace ITECHAutoAttendance;

public class TimeoutException : Exception
{
    public TimeoutException(string message) : base(message) { }
    public override string StackTrace => string.Empty;
}