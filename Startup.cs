using ITECHAutoAttendance.Extensions;
using ITECHAutoAttendance.Jobs;
using ITECHAutoAttendance.Models;
using ITECHAutoAttendance.Sink;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Serilog;
using Serilog.Events;

namespace ITECHAutoAttendance;

public static class Startup
{
    private const string SerilogOutputTemplate =
        "{Timestamp:yyyy'-'MM'-'dd'T'HH':'mm':'ss.ffffff zzz} [{Level:u3}] {SourceContext} - {Message:lj}{NewLine}{Exception}";
    
    public static IHostBuilder CreateHostBuilder() =>
        Host.CreateDefaultBuilder()
            .ConfigureHostConfiguration(hostConfig =>
            {
                hostConfig.AddJsonFileFromDockerVolumeIfExistent("config/appsettings.json");
            })
            .UseSerilog((context, configuration) =>
            {
                configuration.WriteTo.Console(
                    outputTemplate: SerilogOutputTemplate
                );
                
                configuration.WriteTo.Sink(new CaptureLogsSink(), LogEventLevel.Information, null);
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
