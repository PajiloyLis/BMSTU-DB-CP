namespace Project.Core.Exceptions;

public class PositionNotFoundException : Exception
{
    public PositionNotFoundException()
    {
    }

    public PositionNotFoundException(string message) : base(message)
    {
    }

    public PositionNotFoundException(string message, Exception inner) : base(message, inner)
    {
    }
}