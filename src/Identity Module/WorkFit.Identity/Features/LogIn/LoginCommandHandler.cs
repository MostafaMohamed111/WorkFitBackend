using Microsoft.AspNetCore.Identity;
using WorkFit.Identity.AuthServices.Jwt;
using WorkFit.Identity.Domain.Entities;
using WorkFit.Identity.Features.LogIn.Exceptions;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Identity.Features.LogIn;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, string>
{

    private readonly UserManager<WorkFitUser> _userManager;
    private readonly JwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        UserManager<WorkFitUser> userManager,
        JwtTokenGenerator jwtTokenGenerator
        
        )
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
    }
    public async Task<string> Handle(LoginCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(command.Email)
            ?? throw new UserWithThisEmailDoesntExistException(command.Email);

        var token = await _jwtTokenGenerator.GenerateTokenAsync(user);

        return token;
    }
}
