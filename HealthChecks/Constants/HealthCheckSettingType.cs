namespace Observability.HealthChecks.Constants
{
    public enum HealthCheckSettingType : byte
    {
        Non,
        Sql,
        Redis,
        MongoDb,
        RabbitMq,
        Hangfire,
        ExternalApi,
        S3,
        SignalR,
        ElasticSearch,
        Network,
        Grpc,
        SSL
    }
}
