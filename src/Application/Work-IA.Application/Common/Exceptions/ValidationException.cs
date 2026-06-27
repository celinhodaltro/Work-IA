namespace Work_IA.Application.Common.Exceptions;

public sealed class ValidationException : Exception
{
    public ValidationException(string message)
        : base(message)
    {
    }
}
