using Hangfire;
using ITECHAutoAttendance;

GlobalConfiguration.Configuration.UseInMemoryStorage();
var autoAttendance = new AutoAttendance();

BackgroundJob.Enqueue(() => autoAttendance.Start());
RecurringJob.AddOrUpdate(() => autoAttendance.Start(), autoAttendance.CronExpression);

using var _ = new BackgroundJobServer();
Console.WriteLine("Hangfire Server started. Press any key to exit...");
Console.ReadKey();