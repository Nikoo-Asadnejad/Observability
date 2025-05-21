using Observability.HealthChecks.Constants;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Observability.HealthChecks.Models
{
    public class HealthCheckOptions
    {
        public HealthCheckOptions(string applicationName)
        {
            ApplicationName = applicationName;
        }

        private HashSet<string> _healthChecksNames = new HashSet<string>();
        private List<DatabaseConnection> _sqlConnections = new List<DatabaseConnection>();
        private List<DatabaseConnection> _redisConnections = new List<DatabaseConnection>();
        private List<DatabaseConnection> _mongoDbConnections = new List<DatabaseConnection>();
        private List<DatabaseConnection> _elasticSearchConnections = new List<DatabaseConnection>();
        private List<ExternalService> _rabbitMqConnections = new List<ExternalService>();
        private List<ExternalService> _hangfireConnections = new List<ExternalService>();
        private List<ExternalService> _apiConnections = new List<ExternalService>();
        private List<ExternalService> _signalRHubs = new List<ExternalService>();
        private List<ExternalService> _networks = new List<ExternalService>();
        private List<ExternalService> _sslConnections = new List<ExternalService>();
        private List<ExternalService> _grpcServices = new List<ExternalService>();
        private List<S3> _cdns = new List<S3>();

        public string ApplicationName { get; private set; }
        public IReadOnlyList<DatabaseConnection> SqlConnections => _sqlConnections?.AsReadOnly();
        public IReadOnlyList<DatabaseConnection> RedisConnections => _redisConnections?.AsReadOnly();
        public IReadOnlyList<DatabaseConnection> MongoDbConnections => _mongoDbConnections?.AsReadOnly();
        public IReadOnlyList<DatabaseConnection> ElasticSearchConnections => _elasticSearchConnections?.AsReadOnly();
        public IReadOnlyList<ExternalService> RabbitMqConnections => _rabbitMqConnections?.AsReadOnly();
        public IReadOnlyList<ExternalService> HangfireConnections => _hangfireConnections?.AsReadOnly();
        public IReadOnlyList<ExternalService> ApiConnections => _apiConnections?.AsReadOnly();
        public IReadOnlyList<ExternalService> SignalRHubs => _signalRHubs?.AsReadOnly();
        public IReadOnlyList<ExternalService> Networks => _networks?.AsReadOnly();
        public IReadOnlyList<ExternalService> SslConnections => _sslConnections?.AsReadOnly();
        public IReadOnlyList<ExternalService> GrpcServices => _grpcServices?.AsReadOnly();

        public IReadOnlyList<S3> Cdns => _cdns?.AsReadOnly();

        public static HealthCheckOptions CreateFromSetting(HealthCheckSetting setting)
        {
            var options = new HealthCheckOptions(setting?.ApplicationName);

            if (setting is null || setting.Items.Count <= 0)
            {
                return options;
            }

            foreach (var item in setting.Items)
            {
                switch (item.TypeEnum)
                {
                    case HealthCheckSettingType.Sql:
                        options.AddSqlConnection(item.Name, item.Endpoint);
                        break;

                    case HealthCheckSettingType.Redis:
                        options.AddRedisConnection(item.Name, item.Endpoint);
                        break;

                    case HealthCheckSettingType.MongoDb:
                        options.AddMongoDbConnection(item.Name, item.Endpoint);
                        break;

                    case HealthCheckSettingType.RabbitMq:
                        options.AddRabbitMq(item.Name, item.Endpoint);
                        break;

                    case HealthCheckSettingType.Hangfire:
                        options.AddHangfire(item.Name, item.Endpoint, item.AllowedFailureThreshold ?? HealthCheckDefaultValues.MinimumHangfireJobFailure);
                        break;

                    case HealthCheckSettingType.ExternalApi:
                        options.AddApiConnection(item.Name, item.Endpoint);
                        break;

                    case HealthCheckSettingType.SignalR:
                        options.AddSignalRHub(item.Name, item.Endpoint);
                        break;

                    case HealthCheckSettingType.S3:
                        options.AddCdn(item.Name, item.Endpoint, item.CdnBucketName, item.CdnAccessKey, item.CdnSecretKey);
                        break;

                    case HealthCheckSettingType.ElasticSearch:
                        options.AddElasticSearchConnection(item.Name, item.Endpoint, userName: item.UserName, password: item.Password);
                        break;

                    case HealthCheckSettingType.Network:
                        options.AddNetwork(item.Name, item.Endpoint, item.PingTimeoutMilliSecond ?? HealthCheckDefaultValues.PingTimeoutMilliSecond);
                        break;

                    case HealthCheckSettingType.Grpc:
                        options.AddGrpcService(item.Name, item.Endpoint);
                        break;

                    case HealthCheckSettingType.SSL:
                        options.AddSSLConnection(item.Name, item.Endpoint);
                        break;

                    default:
                        break;
                }
            }

            return options;
        }

        public HealthCheckOptions AddSqlConnection(string name, string connectionString)
        {
            if (_sqlConnections == null)
            {
                _sqlConnections = new List<DatabaseConnection>();
            }

            if (IsNotValid(name, connectionString))
            {
                return this;
            }

            _sqlConnections.Add(DatabaseConnection.CreateSQL(name: name, connectionString: connectionString));

            AddHealthCheckName(name);

            return this;
        }

        public HealthCheckOptions AddRedisConnection(string name, string connectionString)
        {
            if (_redisConnections == null)
            {
                _redisConnections = new List<DatabaseConnection>();
            }

            if (IsNotValid(name, connectionString))
            {
                return this;
            }

            _redisConnections.Add(DatabaseConnection.CreateRedis(name: name, connectionString: connectionString));

            AddHealthCheckName(name);

            return this;
        }

        public HealthCheckOptions AddMongoDbConnection(string name, string connectionString)
        {
            if (_mongoDbConnections == null)
            {
                _mongoDbConnections = new List<DatabaseConnection>();
            }

            if (IsNotValid(name, connectionString))
            {
                return this;
            }

            _mongoDbConnections.Add(DatabaseConnection.CreateMongoDB(name: name, connectionString: connectionString));

            AddHealthCheckName(name);

            return this;
        }

        public HealthCheckOptions AddRabbitMq(string name, string url)
        {
            if (_rabbitMqConnections == null)
            {
                _rabbitMqConnections = new List<ExternalService>();
            }

            if (IsNotValid(name, url))
            {
                return this;
            }

            _rabbitMqConnections.Add(ExternalService.CreateRabbitMQ(name: name, url: url));

            AddHealthCheckName(name);

            return this;
        }

        public HealthCheckOptions AddHangfire(string name, string url, int maximumJobFailure)
        {
            if (_hangfireConnections == null)
            {
                _hangfireConnections = new List<ExternalService>();
            }

            if (string.IsNullOrWhiteSpace(name)
                || HealthCheckNameIsDuplicate(name))
            {
                return this;
            }

            _hangfireConnections.Add(ExternalService.CreateHangfire(name: name, url: url, maximumJobFailure: maximumJobFailure));

            AddHealthCheckName(name);

            return this;
        }

        public HealthCheckOptions AddApiConnection(string name, string url)
        {
            if (_apiConnections == null)
            {
                _apiConnections = new List<ExternalService>();
            }

            if (IsNotValid(name, url))
            {
                return this;
            }

            _apiConnections.Add(ExternalService.CreateExternalAPI(name: name, url: url));

            AddHealthCheckName(name);

            return this;
        }

        public HealthCheckOptions AddSignalRHub(string name, string url)
        {
            if (_signalRHubs == null)
            {
                _signalRHubs = new List<ExternalService>();
            }

            if (IsNotValid(name, url))
            {
                return this;
            }

            _signalRHubs.Add(ExternalService.CreateSignalR(name: name, url: url));

            AddHealthCheckName(name);

            return this;
        }

        public HealthCheckOptions AddCdn(string name, string url, string bucketName, string accessKey, string secretKey)
        {
            if (_cdns == null)
            {
                _cdns = new List<S3>();
            }

            if (IsNotValid(name, url)
                || string.IsNullOrWhiteSpace(bucketName)
                || string.IsNullOrWhiteSpace(accessKey)
                || string.IsNullOrWhiteSpace(secretKey))
            {
                return this;
            }

            _cdns.Add(S3.CreateS3(name: name, url: url, bucketName: bucketName, accessKey: accessKey, secretKey: secretKey));

            AddHealthCheckName(name);

            return this;
        }

        public HealthCheckOptions AddElasticSearchConnection(string name, string connectionString, string userName, string password)
        {
            if (_elasticSearchConnections == null)
            {
                _elasticSearchConnections = new List<DatabaseConnection>();
            }

            if (IsNotValid(name, connectionString))
            {
                return this;
            }

            _elasticSearchConnections.Add(DatabaseConnection.CreateElasticSearch(name: name,
                                                                                 connectionString: connectionString,
                                                                                 userName: userName,
                                                                                 password: password));

            AddHealthCheckName(name);

            return this;
        }

        public HealthCheckOptions AddNetwork(string name, string url, int pingTimeoutMilliSecond)
        {
            if (_networks == null)
            {
                _networks = new List<ExternalService>();
            }

            if (IsNotValid(name, url))
            {
                return this;
            }

            _networks.Add(ExternalService.CreateNetwork(name: name, url: url, pingTimeoutMilliSecond: pingTimeoutMilliSecond));

            AddHealthCheckName(name);

            return this;
        }

        public HealthCheckOptions AddGrpcService(string name, string url)
        {
            if (_grpcServices == null)
            {
                _grpcServices = new List<ExternalService>();
            }

            if (IsNotValid(name, url))
            {
                return this;
            }

            _grpcServices.Add(ExternalService.CreateGrpcService(name: name, url: url));

            AddHealthCheckName(name);

            return this;
        }

        public HealthCheckOptions AddSSLConnection(string name, string url)
        {
            if (_apiConnections == null)
            {
                _apiConnections = new List<ExternalService>();
            }

            if (IsNotValid(name, url))
            {
                return this;
            }

            _sslConnections.Add(ExternalService.CreateSSL(name: name, url: url));
            AddHealthCheckName(name);
            return this;
        }

        private bool HealthCheckNameIsDuplicate(string name)
        {
            if (_healthChecksNames == null)
            {
                _healthChecksNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            return !string.IsNullOrWhiteSpace(name) && _healthChecksNames.Contains(name);
        }

        private bool IsNotValid(string name, string url)
        {
            return string.IsNullOrWhiteSpace(name)
                            || string.IsNullOrWhiteSpace(url)
                            || HealthCheckNameIsDuplicate(name);
        }

        private void AddHealthCheckName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            if (_healthChecksNames is null)
            {
                _healthChecksNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            _healthChecksNames.Add(name);
        }

    }

    public class HealthCheckOptionItem
    {
        protected HealthCheckOptionItem(string name, string[] tags)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new Exception("HealthCheck name can not be null.");
            }

            if (tags is null || tags.Length == 0)
            {
                tags = new string[] { name.ToLower() };
            }

            Name = name.ToUpper();
            Tags = tags.Select(t => t.ToLower()).ToArray();
        }

        public string Name { get; private set; }

        public string[] Tags { get; private set; }
    }

    public class DatabaseConnection : HealthCheckOptionItem
    {
        private DatabaseConnection(string name, string[] tags, string connectionString, string userName = null, string password = null) : base(name, tags)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Database connection name cannot be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Database connection string cannot be null or empty.");
            }

            ConnectionString = connectionString;
            UserName = userName;
            Password = password;
        }

        public string ConnectionString { get; private set; }
        public string UserName { get; private set; }
        public string Password { get; private set; }

        internal static DatabaseConnection CreateSQL(string connectionString, string name = "SQL")
        {
            return new DatabaseConnection(name, HealthCheckTags.SQL, connectionString);
        }

        internal static DatabaseConnection CreateRedis(string connectionString, string name = "Redis")
        {
            return new DatabaseConnection(name, HealthCheckTags.Redis, connectionString);
        }

        internal static DatabaseConnection CreateMongoDB(string connectionString, string name = "MongoDB")
        {
            return new DatabaseConnection(name, HealthCheckTags.MongoDB, connectionString);
        }

        internal static DatabaseConnection CreateElasticSearch(string connectionString, string userName, string password, string name = "ElasticSearch")
        {
            return new DatabaseConnection(name, HealthCheckTags.ElasticSearch, connectionString, userName, password);
        }
    }

    public class ExternalService : HealthCheckOptionItem
    {
        protected ExternalService(string name, string[] tags, string url, int? allowedFailureThreshold = null, int? pingTimeoutMilliSecond = null) : base(name, tags)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("API connection name cannot be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("API URL cannot be null or empty.");
            }

            Url = url;
            AllowedFailureThreshold = allowedFailureThreshold;
            PingTimeoutMilliSecond = pingTimeoutMilliSecond;
        }

        public string Url { get; private set; }

        public int? AllowedFailureThreshold { get; private set; }

        public int? PingTimeoutMilliSecond { get; private set; } = 5000;

        protected internal static ExternalService CreateExternalAPI(string url, string name)
        {
            return new ExternalService(name, HealthCheckTags.ExternalAPI(name), url);
        }

        protected internal static ExternalService CreateGrpcService(string url, string name)
        {
            return new ExternalService(name, HealthCheckTags.Grpc(name), url);
        }

        protected internal static ExternalService CreateRabbitMQ(string url, string name = "RabbitMQ")
        {
            return new ExternalService(name, HealthCheckTags.RabbitMQ, FixRabbitMqURL(url));
        }

        protected internal static ExternalService CreateHangfire(string url, string name = "Hangfire", int maximumJobFailure = HealthCheckDefaultValues.MinimumHangfireJobFailure)
        {
            return new ExternalService(name, HealthCheckTags.Hangfire, url, maximumJobFailure);
        }

        protected internal static ExternalService CreateSignalR(string url, string name = "SignalR")
        {
            return new ExternalService(name, HealthCheckTags.SignalR, url);
        }

        protected internal static ExternalService CreateNetwork(string url, string name = "Network", int pingTimeoutMilliSecond = HealthCheckDefaultValues.PingTimeoutMilliSecond)
        {
            return new ExternalService(name, HealthCheckTags.Network, url, pingTimeoutMilliSecond: pingTimeoutMilliSecond);
        }

        protected internal static ExternalService CreateSSL(string url, string name = "SSL Connection")
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("SSL URL cannot be null or empty.");
            }

            return new ExternalService(name, HealthCheckTags.SSL, FixSslEndPoint(url));
        }

        private static string FixRabbitMqURL(string url)
        {
            if (url.StartsWith("amqp://", StringComparison.OrdinalIgnoreCase))
            {
                return url;
            }

            if (url.StartsWith("rabbitmq://", StringComparison.OrdinalIgnoreCase))
            {
                return "amqp://" + url.Substring("rabbitmq://".Length);
            }

            return $"amqp://{url}";
        }

        private static string FixSslEndPoint(string url)
        {
            url = url.Replace("https://", "")
                     .Replace("http://", "");

            if (url.EndsWith("/"))
            {
                url = url.TrimEnd('/');
            }

            return url;
        }
    }

    public class S3 : ExternalService
    {
        private S3(string name, string[] tags, string url, string bucketName, string accessKey, string secretKey) : base(name, tags, url)
        {
            if (string.IsNullOrWhiteSpace(bucketName))
            {
                throw new ArgumentException("S3 bucket name cannot be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(accessKey))
            {
                throw new ArgumentException("S3 access key cannot be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(secretKey))
            {
                throw new ArgumentException("S3 secret key cannot be null or empty.");
            }

            AccessKey = accessKey;
            SecretKey = secretKey;
            BucketName = bucketName;
        }

        public string BucketName { get; private set; }
        public string AccessKey { get; private set; }
        public string SecretKey { get; private set; }


        internal static S3 CreateS3(string url, string bucketName, string accessKey, string secretKey, string name = "S3")
        {
            return new S3(name, HealthCheckTags.S3, url, bucketName, accessKey, secretKey);
        }
    }
}



