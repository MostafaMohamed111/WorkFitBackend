namespace WorkFit.SharedKernel.Exceptions;

public abstract class WorkFitException : Exception
{
    public string Code { get; }
    public string? UserFriendlyMessage { get; set; } = string.Empty;

    protected WorkFitException(string code, string message,string? userFriendlyMessage = null, Exception? inner = null)
        : base(message, inner)
    {
        Code = code;
        UserFriendlyMessage = userFriendlyMessage?? "Something went wrong, please try again later.";
    }
}