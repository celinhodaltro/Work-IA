using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Work_IA.Office.Models;
using Work_IA.Office.SignalR;

namespace Work_IA.Office.Rendering;

public sealed class OfficeRenderer
{
    private readonly GL _gl;
    private readonly OfficeHubClient _hub;
    private Vector2D<int> _size;
    private float _time;
    private readonly Dictionary<string, float> _emotionTimers = [];

    private struct Vertex
    {
        public float X, Y, Z;
        public float R, G, B;
    }

    private uint _vbo, _vao;
    private uint _shader;
    private const string VertexShader = @"
#version 330 core
layout(location = 0) in vec3 aPos;
layout(location = 1) in vec3 aColor;
uniform mat4 uProjection;
out vec3 fColor;
void main() {
    gl_Position = uProjection * vec4(aPos, 1.0);
    fColor = aColor;
}";

    private const string FragmentShader = @"
#version 330 core
in vec3 fColor;
out vec4 FragColor;
void main() {
    FragColor = vec4(fColor, 1.0);
}";

    public OfficeRenderer(GL gl, OfficeHubClient hub, Vector2D<int> size)
    {
        _gl = gl;
        _hub = hub;
        _size = size;
        SetupShaders();
        SetupBuffers();
        gl.ClearColor(0.12f, 0.12f, 0.14f, 1.0f);
        gl.Enable(EnableCap.DepthTest);
    }

    private void SetupShaders()
    {
        var vertex = _gl.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(vertex, VertexShader);
        _gl.CompileShader(vertex);

        var fragment = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(fragment, FragmentShader);
        _gl.CompileShader(fragment);

        _shader = _gl.CreateProgram();
        _gl.AttachShader(_shader, vertex);
        _gl.AttachShader(_shader, fragment);
        _gl.LinkProgram(_shader);

        _gl.DeleteShader(vertex);
        _gl.DeleteShader(fragment);
    }

    private void SetupBuffers()
    {
        _vao = _gl.GenVertexArray();
        _vbo = _gl.GenBuffer();
        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 24, 0);
        _gl.EnableVertexAttribArray(0);
        _gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 24, 12);
        _gl.EnableVertexAttribArray(1);
    }

    public void Update(float deltaTime)
    {
        _time += deltaTime;
        foreach (var agent in _hub.Agents)
        {
            agent.Update(deltaTime);
        }
    }

    public void Render(float deltaTime)
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _gl.UseProgram(_shader);

        var aspect = (float)_size.X / _size.Y;
        var projection = Matrix4X4.CreateOrthographicOffCenter(-aspect * 600, aspect * 600, -600, 600, -1000, 1000);
        unsafe
        {
            var loc = _gl.GetUniformLocation(_shader, "uProjection");
            var span = new Span<float>(&projection.Row1.X, 16);
            _gl.UniformMatrix4(loc, 1, false, span);
        }

        DrawFloor();
        DrawMeetingRoom();

        foreach (var agent in _hub.Agents)
        {
            DrawAgent(agent, deltaTime);
        }
    }

    private void DrawFloor()
    {
        var floorVerts = new Vertex[]
        {
            new() { X = -600, Y = -600, Z = 0, R = 0.15f, G = 0.15f, B = 0.17f },
            new() { X = 600, Y = -600, Z = 0, R = 0.15f, G = 0.15f, B = 0.17f },
            new() { X = 600, Y = 600, Z = 0, R = 0.15f, G = 0.15f, B = 0.17f },
            new() { X = -600, Y = -600, Z = 0, R = 0.15f, G = 0.15f, B = 0.17f },
            new() { X = 600, Y = 600, Z = 0, R = 0.15f, G = 0.15f, B = 0.17f },
            new() { X = -600, Y = 600, Z = 0, R = 0.15f, G = 0.15f, B = 0.17f },
        };
        DrawVertices(floorVerts);
    }

    private void DrawMeetingRoom()
    {
        var roomVerts = new Vertex[]
        {
            new() { X = -200, Y = -200, Z = 1, R = 0.2f, G = 0.25f, B = 0.3f },
            new() { X = 200, Y = -200, Z = 1, R = 0.2f, G = 0.25f, B = 0.3f },
            new() { X = 200, Y = 200, Z = 1, R = 0.2f, G = 0.25f, B = 0.3f },
            new() { X = -200, Y = -200, Z = 1, R = 0.2f, G = 0.25f, B = 0.3f },
            new() { X = 200, Y = 200, Z = 1, R = 0.2f, G = 0.25f, B = 0.3f },
            new() { X = -200, Y = 200, Z = 1, R = 0.2f, G = 0.25f, B = 0.3f },
        };
        DrawVertices(roomVerts);
    }

    private void DrawAgent(VisualAgent agent, float deltaTime)
    {
        var x = agent.CurrentX;
        var y = agent.CurrentY;
        var size = 20f;
        var pulse = MathF.Sin(_time * 2 + agent.Id.GetHashCode()) * 2;
        size += pulse;

        var (r, g, b) = agent.Emotion switch
        {
            "Happy" => (0.2f, 0.8f, 0.2f),
            "Tired" => (0.8f, 0.8f, 0.2f),
            "Excited" => (0.2f, 0.6f, 1.0f),
            "Stressed" => (0.8f, 0.2f, 0.2f),
            "Celebrating" => (1.0f, 0.8f, 0.0f),
            "Thinking" => (0.6f, 0.4f, 0.8f),
            _ => (0.3f, 0.5f, 0.8f)
        };

        var verts = new Vertex[]
        {
            new() { X = x - size, Y = y - size, Z = 5, R = r, G = g, B = b },
            new() { X = x + size, Y = y - size, Z = 5, R = r, G = g, B = b },
            new() { X = x + size, Y = y + size, Z = 5, R = r, G = g, B = b },
            new() { X = x - size, Y = y - size, Z = 5, R = r, G = g, B = b },
            new() { X = x + size, Y = y + size, Z = 5, R = r, G = g, B = b },
            new() { X = x - size, Y = y + size, Z = 5, R = r, G = g, B = b },
        };
        DrawVertices(verts);
    }

    private void DrawVertices(Vertex[] verts)
    {
        unsafe
        {
            fixed (Vertex* ptr = verts)
            {
                _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(verts.Length * 24), ptr, BufferUsageARB.DynamicDraw);
                _gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)verts.Length);
            }
        }
    }

    public void Resize(Vector2D<int> size)
    {
        _size = size;
        _gl.Viewport(0, 0, (uint)size.X, (uint)size.Y);
    }
}
