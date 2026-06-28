using Work_IA.Domain.Abstractions;

namespace Work_IA.Domain.Agents;

public sealed class AgentPosition : ValueObject
{
    public float X { get; }
    public float Y { get; }
    public float Z { get; }

    public AgentPosition(float x, float y, float z = 0)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public float DistanceTo(AgentPosition other)
    {
        return MathF.Sqrt(
            MathF.Pow(X - other.X, 2) +
            MathF.Pow(Y - other.Y, 2) +
            MathF.Pow(Z - other.Z, 2));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return X;
        yield return Y;
        yield return Z;
    }
}
