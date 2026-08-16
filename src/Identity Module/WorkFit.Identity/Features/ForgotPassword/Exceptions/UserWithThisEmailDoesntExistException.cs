using WorkFit.SharedKernel.Exceptions.FeatureExceptions;

namespace WorkFit.Identity.Features.ForgotPassword.Exceptions;

internal class UserWithThisEmailDoesntExistException : FeatureException
{
    public UserWithThisEmailDoesntExistException(string email) : base(ModuleMarker.ModuleName,
            "USER_WITH_THIS_EMAIL_DOESNT_EXIST",
            $"No user found with the email address: {email}.",
            "No account was found with the email address you provided, please check and try again.")
    {

    }
}