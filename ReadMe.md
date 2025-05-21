# Observability Library for .NET

A lightweight and extensible C# class library that enables **health checks**, **metrics**, **tracing**, and **logging** to be added to any .NET application with minimal setup.

With a single method call during `Startup`, this library integrates observability best practices into your app, providing real-time monitoring and diagnostics for databases, caches, message brokers, APIs, and more.

---

## 🚀 Features

- ✅ Health Checks for:
  - SQL Server
  - Redis
  - MongoDB
  - RabbitMQ
  - Hangfire
  - External APIs
  - S3 Storage
  - Elasticsearch
  - Network (Ping)
  - SignalR hubs
  - Grpc Services
  - SSL Connection
- 📊 Metrics (Pluggable)
- 🔍 Tracing (Pluggable)
- 📝 Logging (Built-in)

---

## 📦 Installation

1. Clone or reference the **Observability** library in your .NET project.
---

## 🔧 Configuration

Add an `observability.json` file in your application root with the following structure:

```json
{
  "HealthCheck": {
    "ApplicationName": "YourAppName",
    "Items": [
      {
        "Type": "Sql",
        "Name": "SQL Database",
        "Endpoint": "your-sql-connection-string"
      },
      {
        "Type": "Redis",
        "Name": "Redis Cache",
        "Endpoint": "your-redis-endpoint"
      },
      {
        "Type": "MongoDb",
        "Name": "MongoDB",
        "Endpoint": "your-mongodb-connection-string"
      },
      {
        "Type": "RabbitMq",
        "Name": "RabbitMQ",
        "Endpoint": "your-rabbitmq-url"
      },
      {
        "Type": "Hangfire",
        "Name": "Hangfire",
        "Endpoint": "redis-endpoint",
        "AllowedFailureThreshold": 50
      },
      {
        "Type": "ExternalApi",
        "Name": "Your API",
        "Endpoint": "https://your-api.com/"
      },
      {
        "Type": "S3",
        "Name": "CDN Storage",
        "Endpoint": "your-s3-url",
        "CdnBucketName": "your-bucket",
        "CdnAccessKey": "your-access-key",
        "CdnSecretKey": "your-secret-key"
      },
      {
        "Type": "ElasticSearch",
        "Name": "ELK",
        "UserName": "your-username",
        "Password": "your-password",
        "Endpoint": "http://your-elastic-url"
      },
      {
        "Type": "Network",
        "Name": "Internal Network",
        "Endpoint": "your-ip",
        "PingTimeoutSecond": 5000
      },
      {
        "Type": "SignalR",
        "Name": "SignalR Hub",
        "Endpoint": "https://your-hub-url"
      },
      {
        "Type": "Grpc",
        "Name": "Book-Grpc",
        "Endpoint": "https://your-grpc"
      },
      {
        "Type": "SSL",
        "Name": "SSL Connection",
        "Endpoint": "https://your-endpoint"
      }
    ]
  },
  "AllowedIPs": [
    "127.0.0.1",
    "::1"
  ]
}
```

## 🛠️ Usage

### Step 1: 
Fill the `observability.json` file with the details of the components you want to monitor. Specify the health check items (e.g., databases, APIs, caches) and their respective endpoints. Additionally, define the allowed IPs that can access the /healthz endpoint. To allow unrestricted access, you can use "*" as a wildcard.


### Step 2: Register Observability Services
In your `Program.cs` or `Startup.cs` (inside `ConfigureServices`):

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddObservability();
}
```

## Step 3: Enable Observability Middleware and add `/healthz` endpoint.

In your `Program.cs` or `Startup.cs` (inside `Configure`):

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.AddObservability();
}
```
