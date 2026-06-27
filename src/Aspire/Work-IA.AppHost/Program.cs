using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache")
    .WithImage("redis", "7-alpine");

var eventBus = builder.AddRabbitMQ("eventbus")
    .WithImage("rabbitmq", "3-management-alpine");

var webApi = builder.AddProject<Projects.Work_IA_WebApi>("webapi")
    .WithReference(cache)
    .WithReference(eventBus)
    .WithEnvironment("ConnectionStrings__DefaultConnection", "Data Source=/data/workia.db")
    .WithEnvironment("EventBus__Provider", "InMemory")
    .WithExternalHttpEndpoints();

builder.Build().Run();
