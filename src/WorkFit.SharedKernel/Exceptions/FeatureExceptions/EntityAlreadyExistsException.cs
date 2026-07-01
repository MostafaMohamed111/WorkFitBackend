

namespace WorkFit.SharedKernel.Exceptions.FeatureExceptions
{
    public sealed class EntityAlreadyExistsException : FeatureException
    {
        public EntityAlreadyExistsException(
            string moduleName,
            string objectName,
            Guid objectId, Exception? inner = null)
            : base(moduleName,
                  $"{objectName.ToUpper()}_ENTITY_ALREADY_EXISTS",
                  $"Entity '{objectName}' with ID {objectId} already exists.",
                  $"{objectName} already exists. Please ensure that it is unique.",
                  inner)
        {
        }
    }
}
