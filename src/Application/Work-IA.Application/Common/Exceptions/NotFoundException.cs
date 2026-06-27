namespace Work_IA.Application.Common.Exceptions;

public sealed class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"'{name}' ({key}) was not found.")
    {
    }
}
