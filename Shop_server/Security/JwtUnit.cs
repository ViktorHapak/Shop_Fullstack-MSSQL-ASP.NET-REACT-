using Azure;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Shop_server.Security
{
    public class JwtUnit
    {
        private readonly string _jwtSecret;
        private readonly string _issuer;
        private readonly string _audience;

        private readonly JwtSecurityTokenHandler _tokenHandler = new();
        private readonly ILogger<JwtUnit> _logger;

        public JwtUnit(IConfiguration configuration)
        {
            _jwtSecret = configuration["Jwt:Secret"] ??
                throw new InvalidOperationException("A Jwt:Secret nincs beállítva.");

            _issuer = configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("A Jwt:Issuert nincs beállítva."); ;

            _audience = configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException("A Jwt:Audience nincs beállítva."); ;

            if (Encoding.UTF8.GetByteCount(_jwtSecret) < 32)
            {
                throw new InvalidOperationException("A JWT titkos kulcsnak legalább 32 bájtosnak kell lennie.");
            }
        }


        public string GenerateJWT(UserDetails userDetails)
        {
            ArgumentNullException.ThrowIfNull(userDetails);

            DateTime issuedAt = DateTime.UtcNow;
            DateTime expiresAt = issuedAt.AddHours(24);

            string tokenId = Guid.NewGuid().ToString();

            List<Claim> claims =
            [
                new Claim(JwtRegisteredClaimNames.Sub,userDetails.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, userDetails.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, userDetails.Login),
                new Claim(JwtRegisteredClaimNames.Jti,tokenId),
            ];

            SigningCredentials credentials = new(CreateSigningKey(), SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                notBefore: issuedAt,
                expires: expiresAt,
                signingCredentials: credentials);

            return _tokenHandler.WriteToken(token);
        }

        public Boolean ValidateJWT(string token)
        {
            try
            {
                ClaimsPrincipal claims = GetClaims(token);

                _tokenHandler.ValidateToken(
                        token,
                        new TokenValidationParameters{
                                ValidateIssuerSigningKey = true,
                                IssuerSigningKey = CreateSigningKey(),
                                ValidateIssuer = true,
                                ValidIssuer = _issuer,
                                ValidateAudience = true,
                                ValidAudience = _audience,
                                ValidateLifetime = true,
                                ClockSkew = TimeSpan.FromMinutes(1)
                        },
                        out SecurityToken validatedToken);

                return true;
            }
            catch (Exception e)
            {
                return false;
            }  
        }


        private SymmetricSecurityKey CreateSigningKey()
        {
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        }


        public ClaimsPrincipal GetClaims(string token)
        {

            ClaimsPrincipal principal =
                _tokenHandler.ValidateToken(
                    token,
                    new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = CreateSigningKey(),

                        ValidateIssuer = true,
                        ValidIssuer = _issuer,

                        ValidateAudience = true,
                        ValidAudience = _audience,

                        ValidateLifetime = true,

                        ClockSkew = TimeSpan.FromMinutes(1)
                    },
                    out SecurityToken validatedToken);

            return principal;
        }

        public string GetJti(string token)
        {
            ClaimsPrincipal claims = GetClaims(token);

            return claims.FindFirstValue(JwtRegisteredClaimNames.Jti)
                ?? throw new SecurityTokenException(
                    "A token nem tartalmaz jti értéket.");
        }

        public int GetUserId(string token)
        {
            ClaimsPrincipal claims = GetClaims(token);

            string subject =
                claims.FindFirstValue(
                    JwtRegisteredClaimNames.Sub)
                ?? claims.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new SecurityTokenException(
                    "A token nem tartalmaz felhasználói azonosítót.");

            if (!int.TryParse(subject, out int userId))
            {
                throw new SecurityTokenException(
                    "A felhasználói azonosító érvénytelen.");
            }

            return userId;
        }

        public string GetLogin(string token)
        {
            ClaimsPrincipal claims = GetClaims(token);

            return claims.FindFirstValue(JwtRegisteredClaimNames.UniqueName)
                ?? throw new SecurityTokenException(
                    "A token nem tartalmaz login értéket.");
        }

        public string GetLoginType(string token)
        {
            ClaimsPrincipal claims = GetClaims(token);

            return claims.FindFirstValue("Login_type")
                ?? throw new SecurityTokenException("A token nem tartalmaz login típust.");
        }

        public string? GetUsername(string token)
        {
            ClaimsPrincipal claims = GetClaims(token);

            return claims.FindFirstValue("Login_type") == "username"
                ? claims.FindFirstValue("login") : null;
        }

        public string? GetEmail(string token)
        {
            ClaimsPrincipal claims = GetClaims(token);

            return claims.FindFirstValue("Login_type") == "email" ? claims.FindFirstValue("login") : null;
        }

        public string? ParseJWT(HttpRequest request)
        {
            string header = request.Headers.Authorization.ToString();

            return header.StartsWith( "Bearer ",StringComparison.OrdinalIgnoreCase)
                ? header["Bearer ".Length..].Trim()
                : null;
        }
    }
}
