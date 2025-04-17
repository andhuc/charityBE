using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;

    public JwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (!string.IsNullOrEmpty(token))
        {
            var userId = ValidateTokenAndExtractUserId(token, context);
            if (userId != null)
            {
                // Attach the user ID to the HttpContext
                context.Items["UserId"] = userId;
            } else
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }
        }

        await _next(context);
    }

    private string? ValidateTokenAndExtractUserId(string token, HttpContext context)
    {
        try
        {
            var configuration = context.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;
            var secret = configuration["JwtSettings:Secret"];
            var key = Encoding.ASCII.GetBytes(secret);

            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
            }, out var validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = jwtToken.Claims.FirstOrDefault(x => x.Type == "id")?.Value;

            return userId;
        }
        catch
        {
            // Token is invalid or an error occurred
            return null;
        }
    }

    public static int GetUserId(HttpContext httpContext)
    {
        return httpContext.Items["UserId"] != null ? int.Parse(httpContext.Items["UserId"].ToString()) : -1;
    }

}
