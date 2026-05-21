public static class CorsExtension
{
  public static IServiceCollection AddCorsDependencies(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddCors(options =>
    {
      options.AddPolicy("AllowFrontend", policy =>
          policy.WithOrigins(configuration["AllowedOrigins"]!.Split(','))
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());
    });
    return services;
  }
}