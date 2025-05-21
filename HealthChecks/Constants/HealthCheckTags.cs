namespace Observability.HealthChecks.Constants
{
    public struct HealthCheckTags
    {
        public static string[] SQL = new[] { "db", "sql" };

        public static string[] Redis = new[] { "db", "cache", "redis" };

        public static string[] MongoDB = new[] { "db", "mongo-db" };

        public static string[] RabbitMQ = new[] { "message-broker", "rabbit-mq" };

        public static string[] Hangfire = new[] { "jobs", "background-job" };

        public static string[] S3 = new[] { "storage", "s3", "cdn" };

        public static string[] SignalR = new[] { "realtime", "signalr" };

        public static string[] ElasticSearch = new[] { "db", "elasticsearch" };

        public static string[] Network = new[] { "network", "connectivity" };

        public static string[] SSL = new[] { "ssl" };

        public static string[] Grpc(string grpcServiceName) => new string[] { grpcServiceName, "grpc", "service" };

        public static string[] ExternalAPI(string externalServiceName) => new[] { externalServiceName, "api" };
    }
}
