namespace Project.Core.Exceptions;

public class PositionAlreadyExistsException : Exception
{
    public PositionAlreadyExistsException()
    {
    }

    public PositionAlreadyExistsException(string message) : base(message)
    {
    }

    public PositionAlreadyExistsException(string message, Exception inner) : base(message, inner)
    {
    }
}