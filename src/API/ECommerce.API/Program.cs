using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using ECommerce.Domain.Ports;
using OtpNet;
using QRCoder;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthenticationDependencies(builder.Configuration);
builder.Services.AddAuthorizationDependencies();
builder.Services.AddInfrastrutureDependencies();
builder.Services.AddApplicationDependencies();
builder.Services.AddLogging();
builder.Services.AddScoped<ResponseMapper>();
builder.Services.AddGraphQLDependencies();
builder.Services.AddCorsDependencies(builder.Configuration);
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

//one secret key per client
byte[] secretKey = KeyGeneration.GenerateRandomKey(OtpHashMode.Sha256);
string base32SecretKey = Base32Encoding.ToString(secretKey);
const string issuer = "OtpAuthDemo";
const string user = "user@example.com";
app.MapGet("otp/qrcode", () =>
{
    string escapedIssuer = Uri.EscapeDataString(issuer);
    string escapedUser = Uri.EscapeDataString(user);
    string otpUri = $"otpauth://totp/{escapedIssuer}:{escapedUser}?secret={base32SecretKey}&issuer={escapedIssuer}&digits=6&period=30";

    using var qrGenerator = new QRCodeGenerator();
    using var qrCodeData = qrGenerator.CreateQrCode(otpUri, QRCodeGenerator.ECCLevel.Q);
    using var qrCode = new PngByteQRCode(qrCodeData);
    byte[] qrCodeImage = qrCode.GetGraphic(6);

    return Results.File(qrCodeImage, "image/png");
});


app.UseAuthentication();
app.UseAuthorization();
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var jti = context.User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

        if (jti != null)
        {
            var blacklistedRepo = context.RequestServices.GetRequiredService<IBlacklistedTokenRepository>();

            var isBlacklisted = await blacklistedRepo.IsBlacklistedAsync(jti);

            if (isBlacklisted)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Token has been revoked! Please try again!"
                });
                return;
            }
        }


        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId != null)
        {
            var dbContext = context.RequestServices.GetRequiredService<AppDbContext>();
            var user = await dbContext.Users.FindAsync(Guid.Parse(userId));

            if (user != null && !user.IsActive)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Your account has been disabled! Contact admin."
                });
                return;
            }
        }
    }
    await next();
});

app.MapGraphQL();
app.MapPost("otp/validate", (ValidateOtpRequest request) =>
{
    var totp = new Totp(secretKey);
    var isValid = totp.VerifyTotp(request.Code, out var timeStepMatched, VerificationWindow.RfcSpecifiedNetworkDelay);

    return Results.Ok(new {isValid});
});
    


app.Run();

internal record ValidateOtpRequest(string Code);