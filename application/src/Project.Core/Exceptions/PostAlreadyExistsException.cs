namespace Project.Core.Exceptions;

public class PostAlreadyExistsException : Exception
{
    public PostAlreadyExistsException() : base("Post already exists")
    {
    }

    public PostAlreadyExistsException(string message) : base(message)
    {
    }

    public PostAlreadyExistsException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}