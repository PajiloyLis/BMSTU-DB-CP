namespace Project.Core.Exceptions;

/// <summary>
/// Exception when employee with same unique fields already exists
/// </summary>
public class EmployeeAlreadyExistsException : Exception
{
    public EmployeeAlreadyExistsException()
    {
    }

    public EmployeeAlreadyExistsException(string message) : base(message)
    {
    }

    public EmployeeAlreadyExistsException(string message, Exception? inner) : base(message, inner)
    {
    }
}