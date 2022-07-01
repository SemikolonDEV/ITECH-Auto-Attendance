using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;

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
    
    public Task Execute(IJobExecutionContext context)
    {
        if (context.CancellationToken.IsCancellationRequested)
            return Task.CompletedTask;

        RunJob();
        if (_configuration.RunOnlyOnce)
        {
            _applicationLifetime.StopApplication();
        }

        return Task.CompletedTask;
    }

    private void RunJob()
    {
        _logger.LogInformation("Started trying to attendance");
        try
        {
            _autoAttendance.Run();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            _logger.LogWarning("Failed to successfully attendance");
            return;
        }
        
        _logger.LogInformation("Attended successfully");
    }
}