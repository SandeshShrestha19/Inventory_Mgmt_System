using ECommerce.Infrastructure.Persistence;
using ECommerce.Domain.Ports;
using ECommerce.Infrastructure.Adapters;
using Microsoft.EntityFrameworkCore;
using ECommerce.API.GraphQL.Queries;
using ECommerce.API.GraphQL.Mutations;
using ECommerce.Application.UseCases;

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL + EF Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Adapters
builder.Services.AddScoped<IProductRepository, ProductAdapter>();
builder.Services.AddScoped<IOrderRepository, OrderAdapter>();
builder.Services.AddScoped<IUserRepository, UserAdapter>();

builder.Services.AddScoped<IAddProductUseCase, AddProductUseCase>();
builder.Services.AddScoped<IUpdateProductUseCase, UpdateProductUseCase>();
builder.Services.AddScoped<IRegisterUserUseCase, RegisterUserUseCase>();
builder.Services.AddScoped<IUpdateUserUseCase, UpdateUserUseCase>();

builder.Services.AddLogging();

// GraphQL
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddFiltering()
    .AddSorting()
    .AddProjections()
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true);

var app = builder.Build();

app.UseHttpsRedirection();

// GraphQL endpoint
app.MapGraphQL();

app.Run();