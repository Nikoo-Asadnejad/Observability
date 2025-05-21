using Amazon.S3;
using Microsoft.Extensions.DependencyInjection;
using Observability.HealthChecks.Constants;
using Observability.HealthChecks.CustomHealthChecks;
using Observability.HealthChecks.Models;
using System;

namespace Observability.HealthChecks
{
    public static class HealthChecksBuilderExtensions
    {
        public static IHealthChecksBuilder AddHangfire(this IHealthChecksBuilder builder, HealthCheckOptions options)
        {
            if (options.HangfireConnections != null)
            {
                foreach (var hangfire in options.HangfireConnections)
                {
                    builder.AddHangfire(
                         op =>
                         {
                             op.MaximumJobsFailed = hangfire.AllowedFailureThreshold ?? HealthCheckDefaultValues.MinimumHangfireJobFailure;
                         },
                         name: hangfire.Name,
                         tags: hangfire.Tags
                    );
                }

            }

            return builder;
        }

        public static IHealthChecksBuilder AddExternalApis(this IHealthChecksBuilder builder, HealthCheckOptions options)
        {
            if (options.ApiConnections != null)
            {
                foreach (var api in options.ApiConnections)
                {
                    builder.AddCheck(
                             name: api.Name,
                             tags: api.Tags,
                             instance: new ExternalApiHealthCheck(api.Url)
                    );
                }
            }

            return builder;
        }

        public static IHealthChecksBuilder AddRabbitMq(this IHealthChecksBuilder builder, HealthCheckOptions options)
        {
            if (options.RabbitMqConnections != null)
            {
                foreach (var rabbit in options.RabbitMqConnections)
                {
                    builder.AddRabbitMQ(
                        async (r) =>
                        {
                            var factory = new RabbitMQ.Client.ConnectionFactory()
                            {
                                Uri = new Uri(rabbit.Url)
                            };

                            return await factory.CreateConnectionAsync();
                        },
                        name: rabbit.Name,
                        tags: rabbit.Tags
                    );
                }
            }

            return builder;
        }

        public static IHealthChecksBuilder AddMongoDb(this IHealthChecksBuilder builder, HealthCheckOptions options)
        {
            if (options.MongoDbConnections != null)
            {
                foreach (var mongo in options.MongoDbConnections)
                {
                    builder.AddMongoDb(
                        mongo.ConnectionString,
                        name: mongo.Name,
                        tags: mongo.Tags
                    );
                }
            }

            return builder;
        }

        public static IHealthChecksBuilder AddRedis(this IHealthChecksBuilder builder, HealthCheckOptions options)
        {
            if (options.RedisConnections != null)
            {
                foreach (var redis in options.RedisConnections)
                {
                    builder.AddRedis(
                        redis.ConnectionString,
                        name: redis.Name,
                        tags: redis.Tags
                    );
                }
            }

            return builder;
        }

        public static IHealthChecksBuilder AddSql(this IHealthChecksBuilder builder, HealthCheckOptions options)
        {
            if (options.SqlConnections != null)
            {
                foreach (var sql in options.SqlConnections)
                {
                    builder.AddSqlServer(
                        sql.ConnectionString,
                        name: sql.Name,
                        tags: sql.Tags
                    );
                }
            }

            return builder;
        }

        public static IHealthChecksBuilder AddCdn(this IHealthChecksBuilder builder, HealthCheckOptions options)
        {
            if (options.Cdns != null)
            {
                foreach (var cdn in options.Cdns)
                {
                    builder.AddS3(
                        setup =>
                        {
                            var s3Config = new AmazonS3Config
                            {
                                ServiceURL = cdn.Url,
                                ForcePathStyle = true
                            };

                            setup.S3Config = s3Config;
                            setup.AccessKey = cdn.AccessKey;
                            setup.SecretKey = cdn.SecretKey;
                            setup.BucketName = cdn.BucketName;
                        },
                        name: cdn.Name,
                        tags: cdn.Tags
                    );
                }
            }

            return builder;
        }

        public static IHealthChecksBuilder AddElasticsearch(this IHealthChecksBuilder builder, HealthCheckOptions options)
        {
            if (options.ElasticSearchConnections != null)
            {
                foreach (var elastic in options.ElasticSearchConnections)
                {
                    builder.AddElasticsearch(
                        setup =>
                        {
                            setup.UseServer(elastic.ConnectionString);
                            if (!string.IsNullOrEmpty(elastic.UserName) && !string.IsNullOrEmpty(elastic.Password))
                            {
                                setup.UseBasicAuthentication(elastic.UserName, elastic.Password);
                            }
                        },
                        name: elastic.Name,
                        tags: elastic.Tags
                    );
                }
            }

            return builder;
        }

        public static IHealthChecksBuilder AddNetwork(this IHealthChecksBuilder builder, HealthCheckOptions options)
        {
            if (options.Networks != null)
            {
                foreach (var network in options.Networks)
                {
                    builder.AddPingHealthCheck(
                        setup =>
                        {
                            setup.AddHost(network.Url, network.PingTimeoutMilliSecond ?? HealthCheckDefaultValues.PingTimeoutMilliSecond);
                        },
                        name: network.Name,
                        tags: network.Tags
                    );
                }
            }

            return builder;
        }

        public static IHealthChecksBuilder AddSignalR(this IHealthChecksBuilder builder, HealthCheckOptions options)
        {
            if (options.SignalRHubs != null)
            {
                foreach (var signlR in options.SignalRHubs)
                {
                    builder.AddSignalRHub(
                        signlR.Url,
                        name: signlR.Name,
                        tags: signlR.Tags
                    );
                }
            }

            return builder;
        }

        public static IHealthChecksBuilder AddSSL(this IHealthChecksBuilder builder, HealthCheckOptions healthCheckOptions)
        {
            if (healthCheckOptions.SslConnections != null)
            {
                foreach (var ssl in healthCheckOptions.SslConnections)
                {
                    builder.AddCheck(name: ssl.Name,
                                     tags: HealthCheckTags.SSL,
                                     instance: new SslHealthCheck(ssl.Url));
                }
            }


            return builder;
        }

        public static IHealthChecksBuilder AddGrpc(this IHealthChecksBuilder builder, HealthCheckOptions options)
        {
            if (options.GrpcServices != null)
            {
                foreach (var grpc in options.GrpcServices)
                {
                    builder.AddCheck(
                        name: grpc.Name,
                        tags: grpc.Tags,
                        instance: new GrpcHealthCheck(grpc.Url, grpc.Name)
                    );
                }
            }

            return builder;
        }
    }
}
