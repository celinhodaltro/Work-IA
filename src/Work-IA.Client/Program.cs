using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Work_IA.Application;
using Work_IA.Application.Agents.Commands;
using Work_IA.Client.Rendering;
using Work_IA.Infrastructure;
using Work_IA.Infrastructure.Persistence;

namespace Work_IA.Client;

public static class Program
{
    public static async Task Main()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((ctx, services) =>
            {
                services.AddApplication();
                services.AddInfrastructure(ctx.Configuration);
                services.AddSingleton<AgentStateEventHandler>();
                services.AddSingleton<OfficeRenderer>();
                services.AddSingleton<UIManager>();
                services.AddHostedService<AgentBehaviorService>();
            })
            .Build();

        using (var scope = host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WorkIaDbContext>();
            db.Database.EnsureCreated();
        }

        await host.StartAsync();

        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(1280, 720);
        options.Title = "AI Office OS";
        options.PreferredDepthBufferBits = 24;
        options.API = GraphicsAPI.Default;

        var window = Window.Create(options);
        var renderer = host.Services.GetRequiredService<OfficeRenderer>();
        var ui = host.Services.GetRequiredService<UIManager>();
        IInputContext? input = null;

        window.Load += () =>
        {
            var gl = GL.GetApi(window);
            renderer.Initialize(gl, window.Size);
            input = window.CreateInput();
            ui.Initialize(gl, window);
        };

        window.Update += (delta) =>
        {
            if (input is not null) renderer.HandleInput(input);
            renderer.Update((float)delta);
            ui.Update((float)delta);
        };

        window.Render += (_) => { renderer.Render(); ui.Render(); };

        window.Resize += (size) => renderer.Resize(size);

        window.Run();
        input?.Dispose();
        ui.Dispose();
        await host.StopAsync();
    }
}
