namespace Observability.HealthChecks.Constants
{
    public static class HealthCheckDefaultValues
    {
        /// <summary>
        /// Default value for the maximum number of failed Hangfire jobs before triggering a health check failure.
        /// </summary>
        public const int MinimumHangfireJobFailure = 50;

        public const int PingTimeoutMilliSecond = 5000;
    }
}
