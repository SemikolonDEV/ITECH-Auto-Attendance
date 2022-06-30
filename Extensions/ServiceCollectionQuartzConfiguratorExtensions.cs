using Microsoft.Extensions.Configuration;
using Quartz;

namespace ITECHAutoAttendance.Extensions;

public static class ServiceCollectionQuartzConfiguratorExtensions
{
    public static void AddJobAndTrigger<T>(
        this IServiceCollectionQuartzConfigurator quartz,
        IConfiguration config, string jobName, bool startNow)
        where T : IJob
    {
        var cronSchedule = config["CronExpression"];

        var jobKey = new JobKey(jobName);
        quartz.AddJob<T>(opts => opts.WithIdentity(jobKey));

        quartz.AddTrigger(opts => opts
            .ForJob(jobKey)
            .WithIdentity(jobName + "-trigger")
            .WithCronSchedule(cronSchedule));

        if (startNow)
        {
            quartz.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity(jobName + "-trigger-now")
                .StartNow());
        }
    }
}