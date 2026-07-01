namespace WorkFit.SharedKernel.Exceptions.FeatureExceptions;

public class FeatureException : WorkFitException
{
    public FeatureException(string moduleName, string code, string errorMessage, string? userFriendlyMessage = null, Exception? inner = null) 
        : base(moduleName, code, errorMessage, userFriendlyMessage, inner)
    {
    }
}
