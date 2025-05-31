namespace Project.Core.Exceptions;

public class PostHistoryNotFoundException : Exception
{
    public PostHistoryNotFoundException()
        : base("Post history record not found")
    {
    }

    public PostHistoryNotFoundException(Guid postId, Guid employeeId)
        : base($"Post history record not found for post {postId} and employee {employeeId}")
    {
    }

    public PostHistoryNotFoundException(string message)
        : base(message)
    {
    }

    public PostHistoryNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}