namespace Claims.Domain.Exceptions;

public class NotFoundException(string message) : Exception(message);

public class ConflictException(string message) : Exception(message);

public class ValidationFailedException(IDictionary<string, string[]> errors)
    : Exception("Uno o más campos no son válidos.")
{
    public IDictionary<string, string[]> Errors { get; } = errors;
}
