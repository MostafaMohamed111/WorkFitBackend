namespace WorkFit.SharedKernel.Exceptions.FeatureExceptions;

public sealed class ConcurrencyConflictException : FeatureException
{
    public ConcurrencyConflictException(
        string moduleName,
        string objectName,
        Guid objectId,
        Exception? inner = null)
        : base(
            moduleName,
            $"{objectName.ToUpper()}_CONCURRENCY_CONFLICT",
            $"Entity '{objectName}' with ID {objectId} was changed or deleted by another operation.",
            "The project was changed by another operation. Refresh the project and try again.",
            inner)
    {
    }
}
