namespace Project.Core.Exceptions;

/// <summary>
/// Exception when employee required id doesn't exist
/// </summary>
public class EmployeeNotFoundException : Exception
{
    public EmployeeNotFoundException()
    {
    }

    public EmployeeNotFoundException(string? message) : base(message)
    {
    }

    public EmployeeNotFoundException(string? message, Exception? inner) : base(message, inner)
    {
    }
}