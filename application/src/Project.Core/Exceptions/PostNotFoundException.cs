namespace Project.Core.Exceptions;

public class PostNotFoundException : Exception
{
    public PostNotFoundException() : base("Post not found")
    {
    }

    public PostNotFoundException(string message) : base(message)
    {
    }

    public PostNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}