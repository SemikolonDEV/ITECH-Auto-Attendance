using Serilog.Core;
using Serilog.Events;

namespace ITECHAutoAttendance.Sink
{
    public class CaptureLogsSink : ILogEventSink
    {
        public static List<LogEvent> Logs { get; } = new();

        public void Emit(LogEvent logEvent) => Logs.Add(logEvent);
    }
}