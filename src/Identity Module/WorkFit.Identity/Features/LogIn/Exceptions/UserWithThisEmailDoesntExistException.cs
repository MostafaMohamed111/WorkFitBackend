

using WorkFit.SharedKernel.Exceptions.FeatureExceptions;

namespace WorkFit.Identity.Features.LogIn.Exceptions;

internal class UserWithThisEmailDoesntExistException : FeatureException
{
    public UserWithThisEmailDoesntExistException(string email) : base(ModuleMarker.ModuleName,
            "USER_WITH_THIS_EMAIL_DOESNT_EXIST",
            $"No user found with the email address: {email}.",
            "Wrong email address or password, please try again.")
    {
        
    }
}
