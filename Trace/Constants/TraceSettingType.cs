namespace Observability.Trace.Constants
{
    public enum TraceSettingType : byte
    {
        Non,
        Sql,
        Redis,
        MongoDb,
        RabbitMq,
        Hangfire,
        ElasticSearch,
    }
}
