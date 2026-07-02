using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Work_IA.Application;
using Work_IA.Client.Rendering;
using Work_IA.Infrastructure;
using Work_IA.Infrastructure.Persistence;

namespace Work_IA.Client;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var config = new ConfigManager();

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((ctx, services) =>
            {
                services.AddApplication();
                services.AddInfrastructure(ctx.Configuration);
                services.AddSingleton(config);
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
        options.Size = new Vector2D<int>(800, 600);
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
            ui.Initialize(gl, window, input);
        };

        window.Update += (dt) =>
        {
            if (input is not null)
            {
                renderer.HandleInput(input);
                ui.HandleInput(input);
            }
            if (!ui.IsLauncherMode) renderer.Update((float)dt);
            ui.Update((float)dt);
        };

        window.Render += (_) =>
        {
            var gl = GL.GetApi(window);
            gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            if (!ui.IsLauncherMode) renderer.Render();
            ui.Render();
        };

        window.Resize += (size) => renderer.Resize(size);

        window.Run();
        input?.Dispose();
        ui.Dispose();
        await host.StopAsync();
    }
}
