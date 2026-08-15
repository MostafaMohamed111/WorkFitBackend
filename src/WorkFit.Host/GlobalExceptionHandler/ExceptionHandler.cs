using Microsoft.AspNetCore.Diagnostics;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Host.GlobalExceptionHandler;

internal sealed class ExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, errorCode, errorMessage, userFriendlyMessage) = exception switch
        {
            EntityNotFoundException ex => (StatusCodes.Status404NotFound, ex.Code, ex.Message, ex.UserFriendlyMessage),
            EntityAlreadyExistsException ex => (StatusCodes.Status409Conflict, ex.Code, ex.Message, ex.UserFriendlyMessage),
            ConcurrencyConflictException ex => (StatusCodes.Status409Conflict, ex.Code, ex.Message, ex.UserFriendlyMessage),
            DomainException ex => (StatusCodes.Status400BadRequest, ex.Code, ex.Message, ex.UserFriendlyMessage),
            FeatureException ex => (StatusCodes.Status400BadRequest, ex.Code, ex.Message, ex.UserFriendlyMessage),
            UnauthorizedAccessException ex => (StatusCodes.Status403Forbidden, "FORBIDDEN", ex.Message, "You are not allowed to perform this operation."),
            InvalidOperationException ex => (StatusCodes.Status400BadRequest, "INVALID_OPERATION", ex.Message, ex.Message),
            ArgumentException ex => (StatusCodes.Status400BadRequest, "INVALID_ARGUMENT", ex.Message, ex.Message),
            _ => (StatusCodes.Status500InternalServerError, "INTERNAL_SERVER_ERROR", exception.Message, "Please try again later.")
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(new
        {
            errorCode,
            errorMessage,
            userFriendlyMessage
        }, cancellationToken: cancellationToken);

        return true;    
    }
}
