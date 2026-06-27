using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Work_IA.Application;
using Work_IA.Infrastructure;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddApplication();
        services.AddInfrastructure(context.Configuration);
    })
    .Build();

await host.RunAsync();
