using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace InscricaoChll.Api.Options;

public class JwtTokenOptions
{
    public string SecretKey { get; set; }

    public string Issuer { get; set; }

    public string Subject { get; set; }

    public string Audience { get; set; }

    public DateTime NotBefore => DateTime.UtcNow;

    public DateTime IssuedAt => DateTime.UtcNow;

    public TimeSpan ValidFor => TimeSpan.FromHours(2);

    public DateTime Expiration => IssuedAt.Add(ValidFor);

    public Func<Task<string>> JtiGenerator => () => Task.FromResult(Guid.NewGuid().ToString());

    public SymmetricSecurityKey IssuerSigningKey => new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey));

    public SigningCredentials SigningCredentials => new SigningCredentials(IssuerSigningKey, SecurityAlgorithms.HmacSha256);
}