using Silk.NET.Input;
using Silk.NET.Maths;

namespace Work_IA.Client.Models;

public sealed class OrbitCamera
{
    private float _targetDistance = 800;
    private float _currentDistance = 800;
    public float Yaw { get; private set; }
    public float Pitch { get; private set; } = 30;
    public float TargetX { get; private set; }
    public float TargetY { get; private set; }
    public float TargetZ { get; private set; }

    private float _prevMouseX, _prevMouseY;
    private bool _isRotating, _isPanning;

    public void Update(float deltaTime)
    {
        var speed = 5f * deltaTime;
        _currentDistance += (_targetDistance - _currentDistance) * Math.Clamp(speed, 0, 1);
    }

    public float Distance => _currentDistance;

    public Matrix4X4<float> GetViewMatrix()
    {
        var yawRad = Yaw * MathF.PI / 180;
        var pitchRad = Pitch * MathF.PI / 180;
        var cosPitch = MathF.Cos(pitchRad);
        var x = _currentDistance * cosPitch * MathF.Sin(yawRad);
        var y = _currentDistance * MathF.Sin(pitchRad);
        var z = _currentDistance * cosPitch * MathF.Cos(yawRad);
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
        var currentX = mouse.Position.X;
        var currentY = mouse.Position.Y;

        if (mouse.IsButtonPressed(MouseButton.Right))
        {
            if (!_isRotating && !_isPanning)
            {
                _prevMouseX = currentX;
                _prevMouseY = currentY;
            }
            _isRotating = true;
            _isPanning = false;
            var dx = currentX - _prevMouseX;
            var dy = currentY - _prevMouseY;
            Yaw += dx * 0.2f;
            Pitch = Math.Clamp(Pitch - dy * 0.2f, -89, 89);
            _prevMouseX = currentX;
            _prevMouseY = currentY;
        }
        else if (mouse.IsButtonPressed(MouseButton.Middle))
        {
            if (!_isPanning && !_isRotating)
            {
                _prevMouseX = currentX;
                _prevMouseY = currentY;
            }
            _isPanning = true;
            _isRotating = false;
            var dx = currentX - _prevMouseX;
            var dy = currentY - _prevMouseY;
            var yawRad = Yaw * MathF.PI / 180;
            var factor = _currentDistance * 0.0015f;
            TargetX += (-MathF.Cos(yawRad) * dx - MathF.Sin(yawRad) * dy) * factor;
            TargetZ += (MathF.Sin(yawRad) * dx - MathF.Cos(yawRad) * dy) * factor;
            _prevMouseX = currentX;
            _prevMouseY = currentY;
        }
        else
        {
            _isRotating = false;
            _isPanning = false;
        }
    }

    public void HandleScroll(float delta)
    {
        _targetDistance = Math.Clamp(_targetDistance - delta * 20, 50, 3000);
    }

    public void MoveW(float amount) { TargetZ += amount; }
    public void MoveS(float amount) { TargetZ -= amount; }
    public void MoveA(float amount) { TargetX -= amount; }
    public void MoveD(float amount) { TargetX += amount; }
}
