using WorkFit.SharedKernel.Exceptions.FeatureExceptions;

namespace WorkFit.Documents.CrossCutting.Exceptions;

public sealed class DocumentOwnershipMismatchException : FeatureException
{
    public DocumentOwnershipMismatchException()
        : base(
            ModuleMarker.ModuleName,
            "DOCUMENT_OWNERSHIP_MISMATCH",
            "The document does not belong to the current user.",
            "This document does not belong to your account.")
    {
    }
}