namespace WorkFit.SharedKernel.Exceptions;

public abstract class WorkFitException : Exception
{
    public string Code { get; }
    public string? UserFriendlyMessage { get; set; } = string.Empty;
    public string ModuleName { get;  }


    protected WorkFitException(string moduleName, string code, string errorMessage,string? userFriendlyMessage = null, Exception? inner = null)
        : base(errorMessage, inner)
    {
        ModuleName = moduleName;
        Code = $"{moduleName.ToUpper()}_{code.ToUpper()}";
        UserFriendlyMessage = userFriendlyMessage?? "Something went wrong, please try again later.";
    }
}