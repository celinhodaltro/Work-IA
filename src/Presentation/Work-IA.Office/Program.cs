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
        var hubClient = new OfficeHubClient();
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(1280, 720);
        options.Title = "AI Office OS";
        options.PreferredDepthBufferBits = 24;
        options.API = GraphicsAPI.Default;

        var window = Window.Create(options);
        OfficeRenderer? renderer = null;
        IInputContext? input = null;

        window.Load += async () =>
        {
            var gl = GL.GetApi(window);
            renderer = new OfficeRenderer(gl, hubClient, window.Size);
            input = window.CreateInput();
            await hubClient.ConnectAsync();
        };

        window.Update += (delta) =>
        {
            if (input is not null)
                renderer?.HandleInput(input);
            renderer?.Update((float)delta);
        };

        window.Render += (_) => renderer?.Render();
        window.Resize += (size) => renderer?.Resize(size);

        window.Run();
        input?.Dispose();
        await hubClient.DisposeAsync();
    }
}
