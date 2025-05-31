namespace Project.Core.Exceptions;

/// <summary>
/// Exception thrown when a position history record is not found
/// </summary>
public class PositionHistoryNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PositionHistoryNotFoundException"/> class
    /// </summary>
    public PositionHistoryNotFoundException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PositionHistoryNotFoundException"/> class
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public PositionHistoryNotFoundException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PositionHistoryNotFoundException"/> class
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public PositionHistoryNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}