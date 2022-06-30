using ITECHAutoAttendance.Extensions;
using ITECHAutoAttendance.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Serilog;

namespace ITECHAutoAttendance;

public static class Startup
{
    public static IHostBuilder CreateHostBuilder() =>
        Host.CreateDefaultBuilder()
            .UseSerilog((context, configuration) =>
            {
                configuration.WriteTo.Console();
            })
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<AutoAttendance>();
                
                services.AddQuartz(q =>  
                {
                    q.UseMicrosoftDependencyInjectionJobFactory();
                    q.AddJobAndTrigger<AttendanceJob>(context.Configuration, "AutoAttendanceJob", true);
                });

                services.AddQuartzHostedService(
                    q => q.WaitForJobsToComplete = true);
            });
}