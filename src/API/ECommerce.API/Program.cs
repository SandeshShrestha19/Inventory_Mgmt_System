using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using ECommerce.API.GraphQL.Queries;
using ECommerce.API.GraphQL.Mutations;
using ECommerce.API.GraphQL.Types;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using ECommerce.Domain.Ports;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(builder.Configuration["AllowedOrigins"]!.Split(','))
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

builder.Services.AddAuthentication(options =>
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
        ValidIssuer = builder.Configuration["JwtConfig:Issuer"], //who created the token
        ValidAudience = builder.Configuration["JwtConfig:Audience"], //who used the token
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtConfig:Key"]!)), //secret key to verify token
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true, //check if token is expired
        ValidateIssuerSigningKey = true
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var cookieToken = context.Request.Cookies["token"];
            if (!string.IsNullOrEmpty(cookieToken))
            {
                context.Token = cookieToken;  //read token
            } // if cookie is empty → automatically falls back to Authorization header
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ActiveUser", policy =>
    {
        policy.RequireAssertion(context => 
            context.User.HasClaim(c => c.Type =="IsActive" && c.Value =="true")&&
            context.User.HasClaim(c => c.Type == "IsLoggedIn" && c.Value =="true"));
    });
});
builder.Services.AddInfrastrutureDependencies();
builder.Services.AddApplicationDependencies();
builder.Services.AddLogging();
builder.Services.AddScoped<ResponseMapper>();


builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddType<UserType>()
    .AddAuthorization()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .AddErrorFilter<AuthorizationErrorFilter>()
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true);
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.Use(async (context, next) =>
{
   if (context.User.Identity?.IsAuthenticated == true)
    {
        var jti = context.User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

        if(jti != null)
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

            if(user != null && !user.IsActive)
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

app.Run();