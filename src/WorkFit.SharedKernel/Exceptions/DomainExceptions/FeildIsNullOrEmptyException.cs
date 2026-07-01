
namespace WorkFit.SharedKernel.Exceptions.DomainExceptions
{
    public sealed class FeildIsNullOrEmptyException : DomainException
    {
        public FeildIsNullOrEmptyException(
                string moduleName,
                string objectName,
                string fieldName
            ): base(
                moduleName,
                $"{objectName.ToUpper()}_{fieldName.ToUpper()}_IS_NULL_OR_EMPTY",
                $"{fieldName} in {objectName} is null or empty.",
                $"{fieldName} cannot be empty, please provide a valid value."
            )
        {
            
        }
    }
}
