using Microsoft.Extensions.Configuration;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Work_IA.Office.Rendering;
using Work_IA.Office.SignalR;

namespace Work_IA.Office;

public static class Program
{
    public static async Task Main()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var hubUrl = config["SignalR:HubUrl"] ?? "http://localhost:5000/hub/agent-states";
        var hubClient = new OfficeHubClient();

        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(1280, 720);
        options.Title = "AI Office OS - Escritório";
        options.PreferredDepthBufferBits = 24;

        var window = Window.Create(options);

        OfficeRenderer? renderer = null;

        window.Load += async () =>
        {
            var gl = GL.GetApi(window);
            renderer = new OfficeRenderer(gl, hubClient, window.Size);
            await hubClient.ConnectAsync(hubUrl);
        };

        window.Update += (deltaTime) =>
        {
            renderer?.Update((float)deltaTime);
        };

        window.Render += (deltaTime) =>
        {
            renderer?.Render((float)deltaTime);
        };

        window.Resize += (size) =>
        {
            renderer?.Resize(size);
        };

        window.Run();
        await hubClient.DisposeAsync();
    }
}
