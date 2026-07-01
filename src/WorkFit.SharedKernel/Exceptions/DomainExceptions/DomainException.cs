namespace WorkFit.SharedKernel.Exceptions.DomainExceptions
{
    public  abstract class DomainException : WorkFitException
    {
        protected DomainException(string moduleName, string code, string message, string? userFriendlyMessage = null, Exception? inner = null)
            : base(moduleName, code, message, userFriendlyMessage, inner)
        {
        }
    }
}
