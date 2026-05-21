public static class AuthorizationExtension
{
  public static IServiceCollection AddAuthorizationDependencies(this IServiceCollection services)
  {
    services.AddAuthorization(options =>
    {
        options.AddPolicy("ActiveUser", policy =>
        {
            policy.RequireAssertion(context =>
                context.User.HasClaim(c => c.Type == "IsActive" && c.Value == "true") &&
                context.User.HasClaim(c => c.Type == "IsLoggedIn" && c.Value == "true"));
        });
    });
    return services;
  }
}