using System.Runtime.CompilerServices;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Work_IA.Client.Models;

namespace Work_IA.Client.Rendering;

public sealed class OfficeRenderer : IDisposable
{
    private GL _gl = null!;
    private readonly AgentStateEventHandler _eventHandler;
    private readonly OrbitCamera _camera;
    private Vector2D<int> _size;
    private float _time;
    private uint _shader;
    private uint _vbo, _vao;
    private int _geometryVertexCount;

    private const string Vshader = @"
#version 330 core
layout(location = 0) in vec3 aPos;
layout(location = 1) in vec3 aColor;
layout(location = 2) in vec3 aNormal;
uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProj;
uniform vec3 uLightDir;
out vec3 fColor;
out float fLighting;
void main() {
    gl_Position = uProj * uView * uModel * vec4(aPos, 1.0);
    vec3 normal = normalize(mat3(transpose(inverse(uModel))) * aNormal);
    float diff = max(dot(normal, normalize(uLightDir)), 0.3);
    fColor = aColor;
    fLighting = diff;
}";

    private const string Fshader = @"
#version 330 core
in vec3 fColor;
in float fLighting;
out vec4 FragColor;
void main() { FragColor = vec4(fColor * fLighting, 1.0); }";

    private struct Vertex { public float X, Y, Z, R, G, B, Nx, Ny, Nz; }

    public OfficeRenderer(AgentStateEventHandler eventHandler)
    {
        _eventHandler = eventHandler;
        _camera = new OrbitCamera();
    }

    public void Initialize(GL gl, Vector2D<int> size)
    {
        _gl = gl;
        _size = size;
        gl.ClearColor(0.08f, 0.08f, 0.1f, 1.0f);
        gl.Enable(EnableCap.DepthTest);
        SetupShaders();
        BuildOfficeGeometry();
    }

    private void SetupShaders()
    {
        var v = _gl.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(v, Vshader); _gl.CompileShader(v);
        var f = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(f, Fshader); _gl.CompileShader(f);
        _shader = _gl.CreateProgram();
        _gl.AttachShader(_shader, v); _gl.AttachShader(_shader, f);
        _gl.LinkProgram(_shader);
        _gl.DeleteShader(v); _gl.DeleteShader(f);
    }

    private void BuildOfficeGeometry()
    {
        var verts = new List<Vertex>();
        void AddCube(float x, float y, float z, float w, float h, float d, float r, float g, float bl)
        {
            var hw = w / 2; var hh = h / 2; var hd = d / 2;
            var faces = new[] {
                (0f,0f,-1f, new[] { (-hw,-hh,-hd), ( hw,-hh,-hd), ( hw, hh,-hd), (-hw, hh,-hd) }),
                (0f,0f,1f, new[] { (-hw,-hh, hd), ( hw,-hh, hd), ( hw, hh, hd), (-hw, hh, hd) }),
                (-1f,0f,0f, new[] { (-hw,-hh,-hd), (-hw,-hh, hd), (-hw, hh, hd), (-hw, hh,-hd) }),
                (1f,0f,0f, new[] { ( hw,-hh,-hd), ( hw,-hh, hd), ( hw, hh, hd), ( hw, hh,-hd) }),
                (0f,1f,0f, new[] { (-hw, hh,-hd), ( hw, hh,-hd), ( hw, hh, hd), (-hw, hh, hd) }),
                (0f,-1f,0f, new[] { (-hw,-hh,-hd), ( hw,-hh,-hd), ( hw,-hh, hd), (-hw,-hh, hd) })
            };
            foreach (var (nx, ny, nz, v) in faces)
                foreach (var vi in new[] { 0,1,2, 0,2,3 })
                    verts.Add(new() { X = x + v[vi].Item1, Y = y + v[vi].Item2, Z = z + v[vi].Item3, R = r, G = g, B = bl, Nx = nx, Ny = ny, Nz = nz });
        }

        AddCube(0, -10, 0, 1000, 5, 1000, 0.12f, 0.12f, 0.14f);
        AddCube(0, 150, -500, 1000, 300, 5, 0.18f, 0.18f, 0.2f);
        AddCube(500, 150, 0, 5, 300, 1000, 0.18f, 0.18f, 0.2f);
        AddCube(-500, 150, 0, 5, 300, 1000, 0.18f, 0.18f, 0.2f);

        for (int i = -3; i <= 3; i++)
        {
            AddCube(i * 80, 20, -300, 60, 5, 40, 0.3f, 0.25f, 0.2f);
            AddCube(i * 80 - 25, 5, -320, 5, 15, 5, 0.2f, 0.2f, 0.2f);
            AddCube(i * 80 + 25, 5, -320, 5, 15, 5, 0.2f, 0.2f, 0.2f);
            AddCube(i * 80 - 25, 5, -280, 5, 15, 5, 0.2f, 0.2f, 0.2f);
            AddCube(i * 80 + 25, 5, -280, 5, 15, 5, 0.2f, 0.2f, 0.2f);
            AddCube(i * 80, 20, 300, 60, 5, 40, 0.3f, 0.25f, 0.2f);
            AddCube(i * 80 - 25, 5, 280, 5, 15, 5, 0.2f, 0.2f, 0.2f);
            AddCube(i * 80 + 25, 5, 280, 5, 15, 5, 0.2f, 0.2f, 0.2f);
            AddCube(i * 80 - 25, 5, 320, 5, 15, 5, 0.2f, 0.2f, 0.2f);
            AddCube(i * 80 + 25, 5, 320, 5, 15, 5, 0.2f, 0.2f, 0.2f);
        }

        AddCube(0, 20, 0, 200, 5, 100, 0.25f, 0.3f, 0.35f);
        AddCube(-120, 12, 0, 20, 10, 20, 0.4f, 0.15f, 0.15f);
        AddCube(120, 12, 0, 20, 10, 20, 0.4f, 0.15f, 0.15f);
        AddCube(0, 12, -60, 20, 10, 20, 0.4f, 0.15f, 0.15f);
        AddCube(0, 12, 60, 20, 10, 20, 0.4f, 0.15f, 0.15f);
        AddCube(-450, 20, -450, 20, 40, 20, 0.1f, 0.6f, 0.1f);
        AddCube(450, 20, -450, 20, 40, 20, 0.1f, 0.6f, 0.1f);

        _geometryVertexCount = verts.Count;
        var data = new float[_geometryVertexCount * 9];
        for (int i = 0; i < _geometryVertexCount; i++)
        {
            data[i*9] = verts[i].X; data[i*9+1] = verts[i].Y; data[i*9+2] = verts[i].Z;
            data[i*9+3] = verts[i].R; data[i*9+4] = verts[i].G; data[i*9+5] = verts[i].B;
            data[i*9+6] = verts[i].Nx; data[i*9+7] = verts[i].Ny; data[i*9+8] = verts[i].Nz;
        }

        _vao = _gl.GenVertexArray(); _vbo = _gl.GenBuffer();
        _gl.BindVertexArray(_vao); _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        unsafe { fixed (float* ptr = data) _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(data.Length * 4), ptr, BufferUsageARB.StaticDraw); }
        _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 36, 0); _gl.EnableVertexAttribArray(0);
        _gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 36, 12); _gl.EnableVertexAttribArray(1);
        _gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 36, 24); _gl.EnableVertexAttribArray(2);
    }

    public void HandleInput(IInputContext input)
    {
        if (input.Mice.Count > 0) _camera.HandleMouse(input.Mice[0]);
        if (input.Keyboards.Count > 0)
        {
            var kb = input.Keyboards[0];
            if (kb.IsKeyPressed(Key.W)) _camera.MoveS(10);
            if (kb.IsKeyPressed(Key.S)) _camera.MoveW(10);
            if (kb.IsKeyPressed(Key.A)) _camera.MoveA(10);
            if (kb.IsKeyPressed(Key.D)) _camera.MoveD(10);
        }
    }

    public void Update(float deltaTime)
    {
        _camera.Update(deltaTime);
        _time += deltaTime;
        foreach (var agent in _eventHandler.Agents)
        {
            var speed = 40f * deltaTime;
            var dx = agent.TargetX - agent.CurrentX;
            var dy = agent.TargetY - agent.CurrentY;
            var dist = MathF.Sqrt(dx * dx + dy * dy);
            if (dist > 1)
            { agent.CurrentX += (dx / dist) * speed; agent.CurrentY += (dy / dist) * speed; }
            else { agent.CurrentX = agent.TargetX; agent.CurrentY = agent.TargetY; }
        }
    }

    public void Render()
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _gl.UseProgram(_shader);
        var view = _camera.GetViewMatrix();
        var proj = _camera.GetProjection((float)_size.X / _size.Y);
        var viewArr = ToFloatArray(view);
        var projArr = ToFloatArray(proj);
        var modelArr = ToFloatArray(Matrix4X4<float>.Identity);
        unsafe { var vLoc = _gl.GetUniformLocation(_shader, "uView"); var pLoc = _gl.GetUniformLocation(_shader, "uProj");
                 var mLoc = _gl.GetUniformLocation(_shader, "uModel"); var lLoc = _gl.GetUniformLocation(_shader, "uLightDir");
                 fixed (float* pv = viewArr) { _gl.UniformMatrix4(vLoc, 1, false, pv); }
                 fixed (float* pp = projArr) { _gl.UniformMatrix4(pLoc, 1, false, pp); }
                 fixed (float* pm = modelArr) { _gl.UniformMatrix4(mLoc, 1, false, pm); }
                 _gl.Uniform3(lLoc, 0.5f, 0.8f, 0.6f); }
        _gl.BindVertexArray(_vao); _gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)_geometryVertexCount);

        foreach (var agent in _eventHandler.Agents) DrawAgent(agent);
    }

    private void DrawAgent(VisualAgent agent)
    {
        var pulse = MathF.Sin(_time * 2 + agent.Id.GetHashCode()) * 3;
        var (r, g, b) = agent.Emotion switch
        {
            "Happy" => (0.2f, 0.8f, 0.2f), "Tired" => (0.8f, 0.8f, 0.2f),
            "Excited" => (0.2f, 0.6f, 1.0f), "Stressed" => (0.8f, 0.2f, 0.2f),
            "Celebrating" => (1.0f, 0.8f, 0.0f), "Thinking" => (0.6f, 0.4f, 0.8f),
            _ => (0.3f, 0.5f, 0.8f)
        };
        var verts = new Vertex[] {
            new() { X = agent.CurrentX, Y = agent.CurrentY, Z = 25, R = r, G = g, B = b, Nx = 0, Ny = 0, Nz = -1 },
            new() { X = agent.CurrentX + 15 + pulse, Y = agent.CurrentY, Z = 25, R = r, G = g, B = b, Nx = 0, Ny = 0, Nz = -1 },
            new() { X = agent.CurrentX + 15 + pulse, Y = agent.CurrentY + 15 + pulse, Z = 25, R = r, G = g, B = b, Nx = 0, Ny = 0, Nz = -1 },
            new() { X = agent.CurrentX, Y = agent.CurrentY, Z = 25, R = r, G = g, B = b, Nx = 0, Ny = 0, Nz = -1 },
            new() { X = agent.CurrentX + 15 + pulse, Y = agent.CurrentY + 15 + pulse, Z = 25, R = r, G = g, B = b, Nx = 0, Ny = 0, Nz = -1 },
            new() { X = agent.CurrentX, Y = agent.CurrentY + 15 + pulse, Z = 25, R = r, G = g, B = b, Nx = 0, Ny = 0, Nz = -1 }
        };
        unsafe { fixed (Vertex* ptr = verts) { _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo); _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(verts.Length * 36), ptr, BufferUsageARB.DynamicDraw); } }
        _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 36, 0); _gl.EnableVertexAttribArray(0);
        _gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 36, 12); _gl.EnableVertexAttribArray(1);
        _gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 36, 24); _gl.EnableVertexAttribArray(2);
        _gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)verts.Length);
    }

    public void Resize(Vector2D<int> size) { _size = size; _gl.Viewport(0, 0, (uint)size.X, (uint)size.Y); }
    private static float[] ToFloatArray(Matrix4X4<float> m)
    {
        unsafe { return [m.M11, m.M12, m.M13, m.M14, m.M21, m.M22, m.M23, m.M24, m.M31, m.M32, m.M33, m.M34, m.M41, m.M42, m.M43, m.M44]; }
    }

    public void Dispose() { _gl.DeleteBuffer(_vbo); _gl.DeleteVertexArray(_vao); _gl.DeleteProgram(_shader); }
}
