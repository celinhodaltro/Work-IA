using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Work_IA.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    
    public AuthController(IConfiguration config)
    {
        _config = config;
    }
    
    [HttpPost("token")]
    public IActionResult GenerateToken([FromBody] TokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ApiKey))
            return Unauthorized(new { error = "API key required" });
        
        var secret = _config["Jwt:SecretKey"] ?? "default-secret-key-change-me";
        var issuer = _config["Jwt:Issuer"] ?? "Work-IA";
        var audience = _config["Jwt:Audience"] ?? "Work-IA-Clients";
        var expiry = int.TryParse(_config["Jwt:ExpirationHours"], out var hours) ? hours : 24;
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, request.ApiKey),
            new Claim(ClaimTypes.Role, "Agent"),
            new Claim("permissions", "workspace:observe,workspace:read,tasks:execute")
        };
        
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expiry),
            signingCredentials: credentials);
        
        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt = token.ValidTo
        });
    }
}

public sealed class TokenRequest
{
    public string ApiKey { get; set; } = string.Empty;
}
