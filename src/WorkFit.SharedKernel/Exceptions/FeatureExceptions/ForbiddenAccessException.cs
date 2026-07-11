namespace WorkFit.SharedKernel.Exceptions.FeatureExceptions;

public sealed class ForbiddenAccessException : FeatureException
{
    public ForbiddenAccessException(string moduleName, string objectName, string? userFriendlyMessage = null)
        : base(
            moduleName,
            $"{objectName.ToUpper()}_ACCESS_FORBIDDEN",
            $"Current user is not authorized to access '{objectName}'.",
            userFriendlyMessage ?? "You don't have permission to view this.")
    {
    }
}