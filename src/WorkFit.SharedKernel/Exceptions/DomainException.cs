namespace WorkFit.SharedKernel.Exceptions
{
    public  abstract class DomainException : WorkFitException
    {
        protected DomainException(string code, string message, string? userFriendlyMessage = null, Exception? inner = null)
            : base(code, message, userFriendlyMessage, inner)
        {
        }
    }
}
