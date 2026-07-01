

using WorkFit.SharedKernel.Exceptions.FeatureExceptions;

namespace WorkFit.Identity.Features.RegisterOrganization.Exceptions;

internal sealed class PasswordConfirmationMissMatchException : FeatureException
{
    public PasswordConfirmationMissMatchException() 
        :base(ModuleMarker.ModuleName,
            "PASSWORD_CONFIRMATION_MISMATCH",
            "The password and confirmation password do not match.",
            "The password and confirmation password do not match. Please ensure that both fields are identical."
            )
    {
        
    }
}
