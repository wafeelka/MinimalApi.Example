public class TokenService : ITokenService
{
    private TimeSpan  EXPIRY_DURATION = new TimeSpan(0, 30, 0);
    public string BuildToken(string key, string issuer, UserDto userDto)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, userDto.UserName),
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
        var tokenDescriptor = new JwtSecurityToken(issuer, issuer, claims, expires: DateTime.Now.Add(EXPIRY_DURATION), signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}
