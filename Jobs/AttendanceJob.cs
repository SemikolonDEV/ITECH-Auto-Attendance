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
    
    public async Task Execute(IJobExecutionContext context)
    {
        if (context.CancellationToken.IsCancellationRequested)
            return;

        // Since selenium takes a while to start up when running in a docker container we give it wait 5 seconds... 
        await Task.Delay(1000 * 5);
        
        RunJob();
        if (_configuration.RunOnlyOnce)
        {
            _applicationLifetime.StopApplication();
        }
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