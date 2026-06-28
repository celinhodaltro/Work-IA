using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("cache")
    .WithImage("redis", "7-alpine")
    .WithContainerName("workia-redis");

var rabbitMq = builder.AddRabbitMQ("eventbus")
    .WithImage("rabbitmq", "3-management-alpine")
    .WithContainerName("workia-rabbitmq");

var api = builder.AddProject<Projects.Work_IA_WebApi>("webapi")
    .WithReference(redis)
    .WithReference(rabbitMq)
    .WithEnvironment("ConnectionStrings__DefaultConnection", "Data Source=workia.db")
    .WithExternalHttpEndpoints();

builder.Build().Run();
