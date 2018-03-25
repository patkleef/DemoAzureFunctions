using Microsoft.ApplicationInsights;
using Microsoft.Azure.WebJobs.Host;

namespace Demo.Functions.Logging
{
    public static class LogHelper
    {
        private static TelemetryClient _telemetryClient;

        private static TelemetryClient TelemetryClient
        {
            get
            {
                if (_telemetryClient == null)
                {
                    _telemetryClient = new TelemetryClient
                    {
                        InstrumentationKey = "eaa19450-53cf-4828-b38a-ee82b70f658e"
                    };
                }
                return _telemetryClient;
            }
        }

        public static void Log(TraceWriter log, string message)
        {
            log.Info(message);

            TelemetryClient.TrackTrace(message);
        }
    }
}
