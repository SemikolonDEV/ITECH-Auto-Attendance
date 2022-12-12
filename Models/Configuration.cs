namespace ITECHAutoAttendance.Models;

public class Configuration
{
    public string Username { get; init; }
    public string Password { get; init; }
    public string ClassCourseLink { get; init; }
    public string AttendanceName { get; init; }
    public string CronExpression { get; init; }
    public List<List<DateTime>> RangeDatesToAttend { get; init; }
    public bool HideWindow { get; init; }
    public string? RemoteDriverUrl { get; init; }
    public bool RunOnlyOnce { get; init; }
    public bool UseRemoteDriver { get; init; }

    public NotificationConfiguration Notification { get; set; }
}

public class NotificationConfiguration
{
    private bool _enabled;

    public bool Enabled
    {
        get => _enabled && Email is not null;
        set => _enabled = value;
    }

    public string? Email { get; init; }

    public string SmtpHost { get; init; }
    public string EmailPassword { get; init; }

    

}