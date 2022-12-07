using System.Net;
using System.Net.Mail;
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
        
        _logger.LogInformation("Started trying to attendance");
        try
        {
            _autoAttendance.Run();
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to successfully attend. See exception here: {@Error}", e);
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
            await SendMail(wasSuccessful, _configuration.Notification.Email, CaptureLogsSink.Logs.Where(e => e.Timestamp > start));
            _logger.LogInformation("Successfully send confirmation mail");
        }
        catch (Exception e)
        {
            _logger.LogWarning("Failed to send confirmation mail. See exception here: {@Error}", e);
        }
    }

    private static async Task SendMail(bool wasSuccessful, string recipientEmailAddress, IEnumerable<LogEvent> events)
    {
        var (subject, body) = ConstructMailMessage(wasSuccessful, events);

        // These credentials are shared. Thus please dont misuse them. Makes it easier for everyone.
        const string fromMail = "itechautoattendance.notifier@gmail.com";
        const string fromPassword = "msqrklybrwexrehr";

        var message = new MailMessage
        {
            From = new MailAddress(fromMail, "ITECH Auto-Attendance"),
            Priority = MailPriority.Normal,
            Subject = subject,
            Body = body,
            IsBodyHtml = false,
        };

        message.To.Add(new MailAddress(recipientEmailAddress));
        
        var smtpClient = new SmtpClient("smtp.gmail.com")
        {
            Port = 587, 
            Credentials = new NetworkCredential(fromMail, fromPassword),
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
}