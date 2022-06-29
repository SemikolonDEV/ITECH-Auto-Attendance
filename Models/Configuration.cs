namespace ITECHAutoAttendance;

public class Configuration
{
    public string Username { get; init; }
    public string Password { get; init; }
    public string AttendanceBlockName { get; init; }
    public string CronExpression { get; init; }
    public string? RemoteDriverUrl { get; init; }
    public bool UseRemoteDriver { get; init; }
}