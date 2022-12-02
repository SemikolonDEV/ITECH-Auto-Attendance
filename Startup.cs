using ITECHAutoAttendance.Extensions;
using ITECHAutoAttendance.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Serilog;

namespace ITECHAutoAttendance;

public static class Startup
{
    private const string SerilogOutputTemplate =
        "{Timestamp:yyyy'-'MM'-'dd'T'HH':'mm':'ss.ffffff zzz} [{Level:u3}] {SourceContext} - {Message:lj}{NewLine}{Exception}";

    
    public static IHostBuilder CreateHostBuilder() =>
        Host.CreateDefaultBuilder()
            .UseSerilog((context, configuration) =>
            {
                configuration.WriteTo.Console(
                    outputTemplate: SerilogOutputTemplate
                );
            })
            .ConfigureServices((context, services) =>
            {
                var config = context.Configuration.GetSection("AppConfig").Get<Configuration>();
                services.AddScoped(_ => config);
                services.AddSingleton<AutoAttendance>();
                
                services.AddQuartz(q =>  
                {
                    q.UseMicrosoftDependencyInjectionJobFactory();
                    q.AddJobAndTrigger<AttendanceJob>(config, "AutoAttendanceJob", true);
                });

                services.AddQuartzHostedService(
                    q => q.WaitForJobsToComplete = true);
            });
}
