using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace WorkFit.CodeReview.Infrastructure.Services;

public static class PemKeyUtils
{
    public static RsaSecurityKey GetRsaSecurityKey(string privateKeyPem)
    {
        if (string.IsNullOrWhiteSpace(privateKeyPem))
        {
            throw new ArgumentException("GitHub App private key PEM is required.", nameof(privateKeyPem));
        }

        var rsa = RSA.Create();
        try
        {
            rsa.ImportFromPem(privateKeyPem.Trim());
            return new RsaSecurityKey(rsa);
        }
        catch (Exception ex)
        {
            rsa.Dispose();
            throw new InvalidOperationException("Failed to import GitHub App private key PEM.", ex);
        }
    }
}
