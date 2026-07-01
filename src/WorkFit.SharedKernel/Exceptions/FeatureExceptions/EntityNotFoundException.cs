

namespace WorkFit.SharedKernel.Exceptions.FeatureExceptions;

public sealed class EntityNotFoundException : FeatureException
{
    public EntityNotFoundException(
        string moduleName,
        string objectName,
        Guid objectId,
        string? userFriendlyMessage = null, Exception? inner = null) : base(moduleName,
                  $"{objectName.ToUpper()}_ENTITY_NOT_FOUND",
                  $"Entity '{objectName}' with ID {objectId} not found.",
                  userFriendlyMessage,
                  inner)
    {
    }
}
