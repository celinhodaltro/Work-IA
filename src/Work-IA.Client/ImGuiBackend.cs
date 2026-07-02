using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.OpenGL;

namespace Work_IA.Client;

public sealed class ImGuiBackend : IDisposable
{
    private readonly GL _gl;
    private readonly IInputContext _input;
    private uint _fontTexture;
    private uint _shader;
    private int _attribLocationTex, _attribLocationProjMtx;
    private int _attribLocationVtxPos, _attribLocationVtxUV, _attribLocationVtxColor;
    private uint _vbo, _vao, _ebo;
    private int _vertexSize, _indexSize;

    public ImGuiBackend(GL gl, IInputContext input)
    {
        _gl = gl;
        _input = input;
        var ctx = ImGui.CreateContext();
        ImGui.SetCurrentContext(ctx);
        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;

        BuildFontAtlas();
        CreateDeviceObjects();
    }

    private unsafe void BuildFontAtlas()
    {
        var io = ImGui.GetIO();
        io.Fonts.GetTexDataAsRGBA32(out byte* pixels, out int width, out int height, out int bytesPerPixel);

        _fontTexture = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, _fontTexture);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)width, (uint)height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, (nint)pixels);
        io.Fonts.SetTexID((nint)_fontTexture);
    }

    private void CreateDeviceObjects()
    {
        var vertexShader = @"
#version 330 core
layout(location = 0) in vec2 Position;
layout(location = 1) in vec2 UV;
layout(location = 2) in vec4 Color;
uniform mat4 ProjMtx;
out vec2 Frag_UV;
out vec4 Frag_Color;
void main() {
    Frag_UV = UV;
    Frag_Color = Color;
    gl_Position = ProjMtx * vec4(Position.xy, 0, 1);
}";

        var fragmentShader = @"
#version 330 core
in vec2 Frag_UV;
in vec4 Frag_Color;
uniform sampler2D Texture;
out vec4 Out_Color;
void main() {
    Out_Color = Frag_Color * texture(Texture, Frag_UV.st);
}";

        _shader = _gl.CreateProgram();
        var vs = _gl.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(vs, vertexShader);
        _gl.CompileShader(vs);
        var fs = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(fs, fragmentShader);
        _gl.CompileShader(fs);
        _gl.AttachShader(_shader, vs);
        _gl.AttachShader(_shader, fs);
        _gl.LinkProgram(_shader);
        _gl.DeleteShader(vs);
        _gl.DeleteShader(fs);

        _attribLocationTex = _gl.GetUniformLocation(_shader, "Texture");
        _attribLocationProjMtx = _gl.GetUniformLocation(_shader, "ProjMtx");
        _attribLocationVtxPos = _gl.GetAttribLocation(_shader, "Position");
        _attribLocationVtxUV = _gl.GetAttribLocation(_shader, "UV");
        _attribLocationVtxColor = _gl.GetAttribLocation(_shader, "Color");

        _vao = _gl.GenVertexArray();
        _vbo = _gl.GenBuffer();
        _ebo = _gl.GenBuffer();
        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);

        _vertexSize = sizeof(float) * 4 + sizeof(byte) * 4;
        _indexSize = sizeof(ushort);

        _gl.EnableVertexAttribArray((uint)_attribLocationVtxPos);
        _gl.VertexAttribPointer((uint)_attribLocationVtxPos, 2, VertexAttribPointerType.Float, false, (uint)_vertexSize, 0);
        _gl.EnableVertexAttribArray((uint)_attribLocationVtxUV);
        _gl.VertexAttribPointer((uint)_attribLocationVtxUV, 2, VertexAttribPointerType.Float, false, (uint)_vertexSize, 8);
        _gl.EnableVertexAttribArray((uint)_attribLocationVtxColor);
        _gl.VertexAttribPointer((uint)_attribLocationVtxColor, 4, VertexAttribPointerType.UnsignedByte, true, (uint)_vertexSize, 16);
    }

    public void NewFrame(int width, int height, float deltaSeconds)
    {
        var io = ImGui.GetIO();
        io.DisplaySize = new Vector2(width, height);
        io.DisplayFramebufferScale = Vector2.One;
        io.DeltaTime = deltaSeconds > 0 ? deltaSeconds : 1f / 60f;

        if (_input.Mice.Count > 0)
        {
            var mouse = _input.Mice[0];
            io.MousePos = new Vector2(mouse.Position.X, mouse.Position.Y);
            io.MouseDown[0] = mouse.IsButtonPressed(MouseButton.Left);
            io.MouseDown[1] = mouse.IsButtonPressed(MouseButton.Right);
        }
        ImGui.NewFrame();
    }

    public unsafe void Render()
    {
        ImGui.Render();
        var drawData = ImGui.GetDrawData();

        if (drawData.TotalVtxCount == 0) return;

        var io = ImGui.GetIO();
        var fb = io.DisplaySize.X * io.DisplayFramebufferScale.X;
        var orthoProjection = Matrix4x4.CreateOrthographicOffCenter(0, io.DisplaySize.X, io.DisplaySize.Y, 0, -1, 1);
        var proj = new float[16];
        proj[0] = orthoProjection.M11; proj[1] = orthoProjection.M12; proj[2] = orthoProjection.M13; proj[3] = orthoProjection.M14;
        proj[4] = orthoProjection.M21; proj[5] = orthoProjection.M22; proj[6] = orthoProjection.M23; proj[7] = orthoProjection.M24;
        proj[8] = orthoProjection.M31; proj[9] = orthoProjection.M32; proj[10] = orthoProjection.M33; proj[11] = orthoProjection.M34;
        proj[12] = orthoProjection.M41; proj[13] = orthoProjection.M42; proj[14] = orthoProjection.M43; proj[15] = orthoProjection.M44;

        _gl.UseProgram(_shader);
        _gl.Uniform1(_attribLocationTex, 0);
        _gl.UniformMatrix4(_attribLocationProjMtx, 1, false, proj);
        _gl.BindVertexArray(_vao);
        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindTexture(TextureTarget.Texture2D, _fontTexture);

        for (int n = 0; n < drawData.CmdListsCount; n++)
        {
            var cmdList = drawData.CmdLists[n];
            var vtxBuffer = (byte*)cmdList.VtxBuffer.Data;
            var idxBuffer = (ushort*)cmdList.IdxBuffer.Data;
            var vtxCount = cmdList.VtxBuffer.Size;
            var idxCount = cmdList.IdxBuffer.Size;

            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vtxCount * _vertexSize), (nint)vtxBuffer, BufferUsageARB.StreamDraw);
            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
            _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(idxCount * _indexSize), (nint)idxBuffer, BufferUsageARB.StreamDraw);

            int idxOffset = 0;
            for (int i = 0; i < cmdList.CmdBuffer.Size; i++)
            {
                var cmd = cmdList.CmdBuffer[i];
                _gl.Scissor((int)cmd.ClipRect.X, (int)(io.DisplaySize.Y - cmd.ClipRect.W), (uint)(cmd.ClipRect.Z - cmd.ClipRect.X), (uint)(cmd.ClipRect.W - cmd.ClipRect.Y));
                _gl.DrawElements(PrimitiveType.Triangles, (uint)cmd.ElemCount, DrawElementsType.UnsignedShort, (nint)(idxOffset * _indexSize));
                idxOffset += (int)cmd.ElemCount;
            }
        }
    }

    public void Dispose()
    {
        _gl.DeleteTexture(_fontTexture);
        _gl.DeleteProgram(_shader);
        _gl.DeleteBuffer(_vbo);
        _gl.DeleteBuffer(_ebo);
        _gl.DeleteVertexArray(_vao);
    }
}
