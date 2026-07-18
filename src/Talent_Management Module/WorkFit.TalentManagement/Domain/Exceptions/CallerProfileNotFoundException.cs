using WorkFit.SharedKernel.Exceptions.FeatureExceptions;

namespace WorkFit.TalentManagement.Domain.Exceptions;

public sealed class CallerProfileNotFoundException : FeatureException
{
    public CallerProfileNotFoundException(string moduleName)
        : base(moduleName, "CALLER_HAS_NO_EMPLOYEE_PROFILE",
            "Current user has no associated employee profile.",
            "Your account isn't linked to an employee profile.")
    {
    }
}