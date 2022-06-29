using Hangfire;
using ITECHAutoAttendance;

GlobalConfiguration.Configuration.UseInMemoryStorage();
const string cronExpression = "0 9 * * *"; // Every day at 9 am
var autoAttendance = new AutoAttendance();

RecurringJob.AddOrUpdate(() => autoAttendance.Start(), cronExpression);

using var _ = new BackgroundJobServer();
Console.WriteLine("Hangfire Server started. Press any key to exit...");
Console.ReadKey();