using Silk.NET.Input;
using Silk.NET.Maths;

namespace Work_IA.Office.Models;

public sealed class OrbitCamera
{
    public float Distance { get; private set; } = 800;
    public float Yaw { get; private set; }
    public float Pitch { get; private set; } = 30;
    public float TargetX { get; set; }
    public float TargetY { get; set; }
    public float TargetZ { get; set; }

    private float _prevMouseX, _prevMouseY;
    private bool _isRotating, _isPanning;

    public Matrix4X4<float> GetViewMatrix()
    {
        var yawRad = Yaw * MathF.PI / 180;
        var pitchRad = Pitch * MathF.PI / 180;
        var cosPitch = MathF.Cos(pitchRad);
        var x = Distance * cosPitch * MathF.Sin(yawRad);
        var y = Distance * MathF.Sin(pitchRad);
        var z = Distance * cosPitch * MathF.Cos(yawRad);
        var eye = new Vector3D<float>(TargetX + x, TargetY + y, TargetZ + z);
        var target = new Vector3D<float>(TargetX, TargetY, TargetZ);
        return Matrix4X4.CreateLookAt(eye, target, Vector3D<float>.UnitY);
    }

    public Matrix4X4<float> GetProjection(float aspect)
    {
        return Matrix4X4.CreatePerspectiveFieldOfView(
            MathF.PI / 4, aspect, 10, 5000);
    }

    public void HandleMouse(IMouse mouse)
    {
        if (mouse.IsButtonPressed(MouseButton.Right))
        {
            _isRotating = true;
            _isPanning = false;
            _prevMouseX = mouse.Position.X;
            _prevMouseY = mouse.Position.Y;
        }
        else if (mouse.IsButtonPressed(MouseButton.Middle))
        {
            _isPanning = true;
            _isRotating = false;
            _prevMouseX = mouse.Position.X;
            _prevMouseY = mouse.Position.Y;
        }
        else
        {
            _isRotating = false;
            _isPanning = false;
        }

        if (_isRotating)
        {
            var dx = mouse.Position.X - _prevMouseX;
            var dy = mouse.Position.Y - _prevMouseY;
            Yaw += dx * 0.3f;
            Pitch = Math.Clamp(Pitch - dy * 0.3f, -89, 89);
            _prevMouseX = mouse.Position.X;
            _prevMouseY = mouse.Position.Y;
        }
        else if (_isPanning)
        {
            var dx = mouse.Position.X - _prevMouseX;
            var dy = mouse.Position.Y - _prevMouseY;

            var yawRad = Yaw * MathF.PI / 180;
            var pitchRad = Pitch * MathF.PI / 180;
            var cosPitch = MathF.Cos(pitchRad);
            var rightX = MathF.Cos(yawRad);
            var rightZ = -MathF.Sin(yawRad);
            var upX = -MathF.Sin(pitchRad) * MathF.Sin(yawRad);
            var upY = cosPitch;
            var upZ = -MathF.Sin(pitchRad) * MathF.Cos(yawRad);
            var speed = Distance * 0.002f;

            TargetX += (-rightX * dx + upX * dy) * speed;
            TargetY += (upY * dy) * speed;
            TargetZ += (-rightZ * dx + upZ * dy) * speed;

            _prevMouseX = mouse.Position.X;
            _prevMouseY = mouse.Position.Y;
        }
    }

    public void HandleScroll(float delta)
    {
        Distance = Math.Clamp(Distance - delta * 50, 50, 3000);
    }
}
