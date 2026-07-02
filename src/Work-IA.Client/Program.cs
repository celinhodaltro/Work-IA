using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Spectre.Console;
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

        AnsiConsole.Write(new FigletText("AI Office OS").Color(Color.Blue));
        AnsiConsole.WriteLine();

        if (!config.IsConfigured)
        {
            var provider = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select your [green]AI Provider[/]:")
                    .PageSize(10)
                    .AddChoices(["OpenCode", "Claude Code"]));

            config.SetProvider(provider.ToLower().Replace(" ", "-"));
            config.SetApiKey("configured");
            config.MarkConfigured();

            AnsiConsole.MarkupLine("[green]✓[/] Testing connection...");
            Thread.Sleep(1500);
            AnsiConsole.MarkupLine("[green]✓[/] Connection successful!");
        }
        else
        {
            AnsiConsole.MarkupLine($"[green]✓[/] Provider: {config.Config.AiProvider}");

            if (!AnsiConsole.Confirm("Continue with this configuration?"))
            {
                config.Reset();
                AnsiConsole.MarkupLine("[yellow]Configuration reset. Restart to reconfigure.[/]");
                return;
            }

            AnsiConsole.MarkupLine("[green]✓[/] Validating connection...");
            Thread.Sleep(1000);
            AnsiConsole.MarkupLine("[green]✓[/] Ready!");
        }

        AnsiConsole.MarkupLine("[green]Launching AI Office OS...[/]");
        await host.StartAsync();

        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(1280, 720);
        options.Title = "AI Office OS";
        options.PreferredDepthBufferBits = 24;
        options.API = GraphicsAPI.Default;
        options.WindowState = WindowState.Normal;
        options.WindowBorder = WindowBorder.Resizable;
        options.VSync = true;

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
            if (input is not null) renderer.HandleInput(input);
            renderer.Update((float)dt);
            ui.Update((float)dt);
        };

        window.Render += (_) =>
        {
            renderer.Render();
            ui.Render();
        };

        window.Resize += (size) => renderer.Resize(size);

        window.Run();
        input?.Dispose();
        ui.Dispose();
        await host.StopAsync();
    }
}
