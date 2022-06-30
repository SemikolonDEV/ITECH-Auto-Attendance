using Microsoft.Extensions.Logging;
using Quartz;

namespace ITECHAutoAttendance.Jobs;

[DisallowConcurrentExecution]
public class AttendanceJob : IJob
{
    private readonly AutoAttendance _autoAttendance;
    private readonly ILogger<AttendanceJob> _logger;
    
    public AttendanceJob(ILogger<AttendanceJob> logger, AutoAttendance autoAttendance)
    {
        _autoAttendance = autoAttendance;
        _logger = logger;
    }
    
    public Task Execute(IJobExecutionContext context)
    {
        if (context.CancellationToken.IsCancellationRequested)
            return Task.CompletedTask;

        _logger.LogInformation("Started trying to attendance");
        try
        {
            _autoAttendance.Run();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            _logger.LogWarning("Failed to successfully attendance");
            return Task.CompletedTask;
        }
        
        _logger.LogInformation("Attended successfully");
        return Task.CompletedTask;
    }
}