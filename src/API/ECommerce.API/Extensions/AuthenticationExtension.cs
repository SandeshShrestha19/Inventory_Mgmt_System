using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

public static class AuthenticationExtension
{
    public static IServiceCollection AddAuthenticationDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; //make it true for production when you want https
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = configuration["JwtConfig:Issuer"], //who created the token
                ValidAudience = configuration["JwtConfig:Audience"], //who used the token
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtConfig:Key"]!)), //secret key to verify token
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true, //check if token is expired
                ValidateIssuerSigningKey = true
            };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // var cookieToken = context.Request.Cookies["token"];
                    // if (!string.IsNullOrEmpty(cookieToken))
                    // {
                    //     context.Token = cookieToken;  //read token
                    // } // if cookie is empty → automatically falls back to Authorization header
                    // return Task.CompletedTask;

                    var request = context.Request;

                    // Normal authenticated session
                    if (request.Cookies.TryGetValue(
                            "token",
                            out var accessToken))
                    {
                        context.Token = accessToken;
                        return Task.CompletedTask;
                    }

                    // Temporary 2FA session
                    if (request.Cookies.TryGetValue(
                            "temp2faToken",
                            out var tempToken))
                    {
                        context.Token = tempToken;
                    }

                    return Task.CompletedTask;
                }
            };
        });
        return services;
    }
}