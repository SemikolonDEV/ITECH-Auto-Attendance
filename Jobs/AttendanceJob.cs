using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using ITECHAutoAttendance.Models;
using ITECHAutoAttendance.Sink;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using Serilog.Events;

namespace ITECHAutoAttendance.Jobs;

[DisallowConcurrentExecution]
public class AttendanceJob : IJob
{
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly AutoAttendance _autoAttendance;
    private readonly Configuration _configuration;
    private readonly ILogger<AttendanceJob> _logger;
    
    public AttendanceJob(ILogger<AttendanceJob> logger,
        AutoAttendance autoAttendance,
        Configuration configuration,
        IHostApplicationLifetime applicationLifetime)
    {
        _applicationLifetime = applicationLifetime;
        _autoAttendance = autoAttendance;
        _configuration = configuration;
        _logger = logger;
    }
    
    public async Task Execute(IJobExecutionContext context)
    {
        if (context.CancellationToken.IsCancellationRequested)
            return;

        // Since selenium takes a while to start up when running in a docker container we give it wait 5 seconds... 
        await Task.Delay(1000 * 5);
        
        await RunJob();
        if (_configuration.RunOnlyOnce) 
            _applicationLifetime.StopApplication();
    }

    private async Task RunJob()
    {
        var start = DateTimeOffset.Now;
        var wasSuccessful = true;

        // Check if there are any lessons today.
        if (!_configuration.RangeDatesToAttend.Any(dates => DateIsBetween(dates.First(), dates.Last(), DateTime.Now)))
        {
            _logger.LogInformation(
                "Not trying to attend today because the current date is not in any datetime range or in other words, following to config auto attendance should not try to attend today because today there are no lessons.");
            return;
        }
        
        _logger.LogInformation("Started trying to attendance");
        try
        {
            _autoAttendance.Run();
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to successfully attend. See error message here: {@ErrorMessage}\n\nAnd full exception here {@Exception}", e.Message, e);
            wasSuccessful = false;
        }

        if (wasSuccessful) _logger.LogInformation("Attended successfully");

        if (!_configuration.Notification.Enabled)
            return;

        if (_configuration.Notification.Email is null)
        {
            _logger.LogWarning("Failed to send confirmation mail. Email notifications are enabled but no email was specified. Please add an email to the appsettings.json.");
            return;
        }
        
        try
        {
            await SendMail(wasSuccessful, _configuration.Notification.EmailPassword, _configuration.Notification.SmtpHost, _configuration.Notification.Email, CaptureLogsSink.Logs.Where(e => e.Timestamp > start));
            _logger.LogInformation("Successfully send confirmation mail");
        }
        catch (Exception e)
        {
            _logger.LogWarning("Failed to send confirmation mail. See exception here: {@Error}", e);
        }
    }

    private static async Task SendMail(bool wasSuccessful, string password, string smtpHost, string recipientEmailAddress, IEnumerable<LogEvent> events)
    {
        var (subject, body) = ConstructMailMessage(wasSuccessful, events);

        var message = new MailMessage
        {
            From = new MailAddress(recipientEmailAddress, "ITECH Auto-Attendance"),
            Priority = MailPriority.Normal,
            Subject = subject,
            Body = body,
            IsBodyHtml = false,
        };

        message.To.Add(new MailAddress(recipientEmailAddress));

        var smtpRegex = Regex.Match(smtpHost, "(?<host>.+):(?<port>[0-9]+)");

        var host = smtpRegex.Groups["host"].Value;
        var smtpPort = int.Parse(smtpRegex.Groups["port"].Value);

        var smtpClient = new SmtpClient(host, smtpPort)
        {
            Credentials = new NetworkCredential(recipientEmailAddress, password),
            EnableSsl = true,
        };

        await smtpClient.SendMailAsync(message);
    }

    private static (string subject, string body) ConstructMailMessage(bool wasSuccessful, IEnumerable<LogEvent> events)
    {
        var logs = events.Select(e => $"[{e.Timestamp:yyyy'-'MM'-'dd'T'HH':'mm':'ss.ffffff zzz}] - {e.RenderMessage()}");
        
        const string success = "attended successfully";
        const string failed = "failed to attend";
        var mailSubject = $"ITECH-Auto-Attendance {(wasSuccessful ? success : failed)}";
        
        const string mailSuccessfulMessage = "Hi friend,\n\nITECH-Auto-Attendance successfully attend for today :D.\n\nView logs here:\n";
        const string mailFailureMessage = "Hi friend,\n\nHate to bring it to you but ITECH-Auto-Attendance failed you once again :/.\n\nYou can check logs here:\n";

        return (mailSubject, (wasSuccessful ? mailSuccessfulMessage : mailFailureMessage) + string.Join('\n', logs));
    }

    private static bool DateIsBetween(DateTime start, DateTime end, DateTime currentDate)
        // We add one day to the end date since on the last day there are still lessons.
        => currentDate.Ticks > start.Ticks && currentDate.Ticks < end.Ticks + TimeSpan.FromDays(1).Ticks;
}