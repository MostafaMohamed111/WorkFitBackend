using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WorkFit.Identity.Domain.Entities;

namespace WorkFit.Identity.AuthServices.Jwt;

public sealed class JwtTokenGenerator
{
    private readonly UserManager<WorkFitUser> _userManager;
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(UserManager<WorkFitUser> userManager,
            IConfiguration configuration
        )
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<string> GenerateTokenAsync(WorkFitUser user)
    {

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName!),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim("organization_id", user.OrganizationId.ToString()!), 

            new Claim(JwtRegisteredClaimNames.Name, user.DisplayName),
        };

        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!));

        var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpiryMinutes"]!)),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}
