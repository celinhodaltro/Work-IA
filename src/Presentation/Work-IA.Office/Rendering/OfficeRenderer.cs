using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System.Runtime.InteropServices;
using Work_IA.Office.Models;
using Work_IA.Office.SignalR;

namespace Work_IA.Office.Rendering;

public sealed class OfficeRenderer : IDisposable
{
    private readonly GL _gl;
    private readonly OfficeHubClient _hub;
    private Vector2D<int> _size;
    private readonly OrbitCamera _camera;
    private float _time;
    private uint _officeVbo, _agentVbo, _vao;
    private uint _shader;
    private int _officeVertexCount;

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
void main() {
    FragColor = vec4(fColor * fLighting, 1.0);
}";

    [StructLayout(LayoutKind.Sequential)]
    private struct Vertex { public float X, Y, Z, R, G, B, Nx, Ny, Nz; }

    public OfficeRenderer(GL gl, OfficeHubClient hub, Vector2D<int> size)
    {
        _gl = gl;
        _hub = hub;
        _size = size;
        _camera = new OrbitCamera();
        SetupShaders();
        gl.ClearColor(0.08f, 0.08f, 0.1f, 1.0f);
        gl.Enable(EnableCap.DepthTest);
        BuildOffice();
    }

    private void SetupShaders()
    {
        uint CompileShader(ShaderType type, string src)
        {
            var s = _gl.CreateShader(type);
            _gl.ShaderSource(s, src);
            _gl.CompileShader(s);
            _gl.GetShader(s, ShaderParameterName.CompileStatus, out int status);
            if (status == 0)
            {
                var log = _gl.GetShaderInfoLog(s);
                throw new InvalidOperationException($"Shader {type} failed: {log}");
            }
            return s;
        }

        var v = CompileShader(ShaderType.VertexShader, Vshader);
        var f = CompileShader(ShaderType.FragmentShader, Fshader);
        _shader = _gl.CreateProgram();
        _gl.AttachShader(_shader, v);
        _gl.AttachShader(_shader, f);
        _gl.LinkProgram(_shader);
        int linkStatus;
        unsafe { _gl.GetProgram(_shader, (GLEnum)0x8B82, &linkStatus); }
        if (linkStatus == 0)
        {
            var log = _gl.GetProgramInfoLog(_shader);
            throw new InvalidOperationException($"Program link failed: {log}");
        }
        _gl.DeleteShader(v);
        _gl.DeleteShader(f);
    }

    private void BuildOffice()
    {
        var verts = new List<Vertex>();

        void AddCube(float x, float y, float z, float w, float h, float d, float r, float g, float bl)
        {
            var hw = w / 2; var hh = h / 2; var hd = d / 2;
            var faces = new (float nx, float ny, float nz, (float x, float y, float z)[] v)[]
            {
                (0,0,-1, new[] { (-hw,-hh,-hd), ( hw,-hh,-hd), ( hw, hh,-hd), (-hw, hh,-hd) }),
                (0,0,1,  new[] { (-hw,-hh, hd), ( hw,-hh, hd), ( hw, hh, hd), (-hw, hh, hd) }),
                (-1,0,0,  new[] { (-hw,-hh,-hd), (-hw,-hh, hd), (-hw, hh, hd), (-hw, hh,-hd) }),
                (1,0,0,   new[] { ( hw,-hh,-hd), ( hw,-hh, hd), ( hw, hh, hd), ( hw, hh,-hd) }),
                (0,1,0,   new[] { (-hw, hh,-hd), ( hw, hh,-hd), ( hw, hh, hd), (-hw, hh, hd) }),
                (0,-1,0,  new[] { (-hw,-hh,-hd), ( hw,-hh,-hd), ( hw,-hh, hd), (-hw,-hh, hd) }),
            };
            foreach (var (nx, ny, nz, vertsF) in faces)
            {
                verts.Add(new() { X = x + vertsF[0].x, Y = y + vertsF[0].y, Z = z + vertsF[0].z, R = r, G = g, B = bl, Nx = nx, Ny = ny, Nz = nz });
                verts.Add(new() { X = x + vertsF[1].x, Y = y + vertsF[1].y, Z = z + vertsF[1].z, R = r, G = g, B = bl, Nx = nx, Ny = ny, Nz = nz });
                verts.Add(new() { X = x + vertsF[2].x, Y = y + vertsF[2].y, Z = z + vertsF[2].z, R = r, G = g, B = bl, Nx = nx, Ny = ny, Nz = nz });
                verts.Add(new() { X = x + vertsF[0].x, Y = y + vertsF[0].y, Z = z + vertsF[0].z, R = r, G = g, B = bl, Nx = nx, Ny = ny, Nz = nz });
                verts.Add(new() { X = x + vertsF[2].x, Y = y + vertsF[2].y, Z = z + vertsF[2].z, R = r, G = g, B = bl, Nx = nx, Ny = ny, Nz = nz });
                verts.Add(new() { X = x + vertsF[3].x, Y = y + vertsF[3].y, Z = z + vertsF[3].z, R = r, G = g, B = bl, Nx = nx, Ny = ny, Nz = nz });
            }
        }

        // Floor
        AddCube(0, -10, 0, 1000, 5, 1000, 0.12f, 0.12f, 0.14f);

        // Walls
        AddCube(0, 150, -500, 1000, 300, 5, 0.18f, 0.18f, 0.2f);
        AddCube(500, 150, 0, 5, 300, 1000, 0.18f, 0.18f, 0.2f);
        AddCube(-500, 150, 0, 5, 300, 1000, 0.18f, 0.18f, 0.2f);

        // Desks row 1 (Z negative)
        for (int i = -3; i <= 3; i++)
        {
            AddCube(i * 80, 20, -300, 60, 5, 40, 0.3f, 0.25f, 0.2f);
            AddCube(i * 80 - 25, 5, -320, 5, 15, 5, 0.2f, 0.2f, 0.2f);
            AddCube(i * 80 + 25, 5, -320, 5, 15, 5, 0.2f, 0.2f, 0.2f);
            AddCube(i * 80 - 25, 5, -280, 5, 15, 5, 0.2f, 0.2f, 0.2f);
            AddCube(i * 80 + 25, 5, -280, 5, 15, 5, 0.2f, 0.2f, 0.2f);
        }

        // Desks row 2 (Z positive)
        for (int i = -3; i <= 3; i++)
        {
            AddCube(i * 80, 20, 300, 60, 5, 40, 0.3f, 0.25f, 0.2f);
            AddCube(i * 80 - 25, 5, 280, 5, 15, 5, 0.2f, 0.2f, 0.2f);
            AddCube(i * 80 + 25, 5, 280, 5, 15, 5, 0.2f, 0.2f, 0.2f);
            AddCube(i * 80 - 25, 5, 320, 5, 15, 5, 0.2f, 0.2f, 0.2f);
            AddCube(i * 80 + 25, 5, 320, 5, 15, 5, 0.2f, 0.2f, 0.2f);
        }

        // Meeting table (center)
        AddCube(0, 20, 0, 200, 5, 100, 0.25f, 0.3f, 0.35f);
        AddCube(-90, 5, -45, 5, 15, 5, 0.2f, 0.2f, 0.2f);
        AddCube(90, 5, -45, 5, 15, 5, 0.2f, 0.2f, 0.2f);
        AddCube(-90, 5, 45, 5, 15, 5, 0.2f, 0.2f, 0.2f);
        AddCube(90, 5, 45, 5, 15, 5, 0.2f, 0.2f, 0.2f);

        // Chairs around meeting table
        AddCube(-120, 12, 0, 20, 10, 20, 0.4f, 0.15f, 0.15f);
        AddCube(120, 12, 0, 20, 10, 20, 0.4f, 0.15f, 0.15f);
        AddCube(0, 12, -60, 20, 10, 20, 0.4f, 0.15f, 0.15f);
        AddCube(0, 12, 60, 20, 10, 20, 0.4f, 0.15f, 0.15f);

        // Plants
        AddCube(-450, 20, -450, 20, 40, 20, 0.1f, 0.6f, 0.1f);
        AddCube(450, 20, -450, 20, 40, 20, 0.1f, 0.6f, 0.1f);
        AddCube(-450, 20, 450, 20, 40, 20, 0.1f, 0.6f, 0.1f);
        AddCube(450, 20, 450, 20, 40, 20, 0.1f, 0.6f, 0.1f);

        // Reception
        AddCube(-450, 20, 0, 60, 5, 40, 0.35f, 0.3f, 0.25f);
        AddCube(-430, 35, 0, 20, 30, 20, 0.4f, 0.35f, 0.3f);

        _officeVertexCount = verts.Count;
        var data = new float[_officeVertexCount * 9];
        for (int i = 0; i < _officeVertexCount; i++)
        {
            data[i * 9] = verts[i].X; data[i * 9 + 1] = verts[i].Y; data[i * 9 + 2] = verts[i].Z;
            data[i * 9 + 3] = verts[i].R; data[i * 9 + 4] = verts[i].G; data[i * 9 + 5] = verts[i].B;
            data[i * 9 + 6] = verts[i].Nx; data[i * 9 + 7] = verts[i].Ny; data[i * 9 + 8] = verts[i].Nz;
        }

        // Setup VAO with both VBOs
        _vao = _gl.GenVertexArray();
        _officeVbo = _gl.GenBuffer();
        _agentVbo = _gl.GenBuffer();

        _gl.BindVertexArray(_vao);

        // Upload office geometry
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _officeVbo);
        unsafe
        {
            fixed (float* ptr = data)
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(data.Length * 4), ptr, BufferUsageARB.StaticDraw);
        }

        // Configure vertex attributes (stride = 9 floats = 36 bytes)
        _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 36, 0);
        _gl.EnableVertexAttribArray(0);
        _gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 36, 12);
        _gl.EnableVertexAttribArray(1);
        _gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 36, 24);
        _gl.EnableVertexAttribArray(2);

        // Upload empty agent buffer
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _agentVbo);
        unsafe
        {
            var empty = new float[6 * 9];
            fixed (float* ptr = empty)
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(empty.Length * 4), ptr, BufferUsageARB.DynamicDraw);
        }
    }

    public void HandleInput(IInputContext input)
    {
        if (input.Mice.Count > 0)
        {
            var mouse = input.Mice[0];
            _camera.HandleMouse(mouse);
            mouse.Scroll += (_, scroll) => _camera.HandleScroll(scroll.Y);
        }
        if (input.Keyboards.Count > 0)
        {
            var kb = input.Keyboards[0];
            if (kb.IsKeyPressed(Key.W)) _camera.TargetZ -= 10;
            if (kb.IsKeyPressed(Key.S)) _camera.TargetZ += 10;
            if (kb.IsKeyPressed(Key.A)) _camera.TargetX -= 10;
            if (kb.IsKeyPressed(Key.D)) _camera.TargetX += 10;
        }
    }

    public void Update(float deltaTime)
    {
        _time += deltaTime;
        foreach (var agent in _hub.Agents)
            agent.Update(deltaTime);
    }

    public void Render()
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _gl.UseProgram(_shader);

        var view = _camera.GetViewMatrix();
        var proj = _camera.GetProjection((float)_size.X / _size.Y);
        var lightDir = new Vector3D<float>(0.5f, 0.8f, 0.6f);

        unsafe
        {
            var viewLoc = _gl.GetUniformLocation(_shader, "uView");
            var projLoc = _gl.GetUniformLocation(_shader, "uProj");
            var modelLoc = _gl.GetUniformLocation(_shader, "uModel");
            var lightLoc = _gl.GetUniformLocation(_shader, "uLightDir");

            float* pv = &view.Row1.X; _gl.UniformMatrix4(viewLoc, 1, false, pv);
            float* pp = &proj.Row1.X; _gl.UniformMatrix4(projLoc, 1, false, pp);
            _gl.Uniform3(lightLoc, lightDir.X, lightDir.Y, lightDir.Z);

            var model = Matrix4X4<float>.Identity;
            float* pm = &model.Row1.X; _gl.UniformMatrix4(modelLoc, 1, false, pm);
        }

        // Draw office geometry
        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _officeVbo);
        _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 36, 0);
        _gl.EnableVertexAttribArray(0);
        _gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 36, 12);
        _gl.EnableVertexAttribArray(1);
        _gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 36, 24);
        _gl.EnableVertexAttribArray(2);
        _gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)_officeVertexCount);

        // Draw agents
        foreach (var agent in _hub.Agents)
            DrawAgent(agent);
    }

    private void DrawAgent(VisualAgent agent)
    {
        var pulse = MathF.Sin(_time * 2 + agent.Id.GetHashCode()) * 3;
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

        // FIX: agent.CurrentY maps to world Z (depth), agent height is fixed at Y=15
        var ax = agent.CurrentX;
        var ay = 15f;
        var az = agent.CurrentY;
        var s = 15f + pulse;

        var verts = new Vertex[]
        {
            new() { X = ax - s, Y = ay - s, Z = az, R = r, G = g, B = b, Nx = 0, Ny = 0, Nz = -1 },
            new() { X = ax + s, Y = ay - s, Z = az, R = r, G = g, B = b, Nx = 0, Ny = 0, Nz = -1 },
            new() { X = ax + s, Y = ay + s, Z = az, R = r, G = g, B = b, Nx = 0, Ny = 0, Nz = -1 },
            new() { X = ax - s, Y = ay - s, Z = az, R = r, G = g, B = b, Nx = 0, Ny = 0, Nz = -1 },
            new() { X = ax + s, Y = ay + s, Z = az, R = r, G = g, B = b, Nx = 0, Ny = 0, Nz = -1 },
            new() { X = ax - s, Y = ay + s, Z = az, R = r, G = g, B = b, Nx = 0, Ny = 0, Nz = -1 },
        };

        // Bind agent VBO, upload, reconfigure attrib pointers, and draw
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _agentVbo);
        unsafe
        {
            fixed (Vertex* ptr = verts)
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(verts.Length * 36), ptr, BufferUsageARB.DynamicDraw);
        }
        _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 36, 0);
        _gl.EnableVertexAttribArray(0);
        _gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 36, 12);
        _gl.EnableVertexAttribArray(1);
        _gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 36, 24);
        _gl.EnableVertexAttribArray(2);
        _gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)verts.Length);
    }

    public void Resize(Vector2D<int> size)
    {
        _size = size;
        _gl.Viewport(0, 0, (uint)size.X, (uint)size.Y);
    }

    public void Dispose()
    {
        _gl.DeleteBuffer(_officeVbo);
        _gl.DeleteBuffer(_agentVbo);
        _gl.DeleteVertexArray(_vao);
        _gl.DeleteProgram(_shader);
    }
}
