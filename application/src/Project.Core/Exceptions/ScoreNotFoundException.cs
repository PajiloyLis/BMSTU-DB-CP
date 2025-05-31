namespace Project.Core.Exceptions;

/// <summary>
/// Exception when score for required employee doesn't exist
/// </summary>
public class ScoreNotFoundException : Exception
{
    public ScoreNotFoundException() : base("Score not found")
    {
    }

    public ScoreNotFoundException(string message) : base(message)
    {
    }

    public ScoreNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}