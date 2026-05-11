using ECommerce.Infrastructure.Persistence;
using ECommerce.Domain.Ports;
using ECommerce.Infrastructure.Adapters;
using Microsoft.EntityFrameworkCore;
using ECommerce.API.GraphQL.Queries;
using ECommerce.API.GraphQL.Mutations;
using ECommerce.Application.UseCases;
using ECommerce.API.GraphQL.Types;

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL + EF Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Adapters
builder.Services.AddScoped<IProductRepository, ProductAdapter>();
builder.Services.AddScoped<IOrderRepository, OrderAdapter>();
builder.Services.AddScoped<IUserRepository, UserAdapter>();

builder.Services.AddScoped<IAddProductUseCase, AddProductUseCase>();
builder.Services.AddScoped<IUpdateProductUseCase, UpdateProductUseCase>();
builder.Services.AddScoped<IRegisterUserUseCase, RegisterUserUseCase>();
builder.Services.AddScoped<IUpdateUserUseCase, UpdateUserUseCase>();
builder.Services.AddScoped<IPlaceOrderUseCase, PlaceOrderUseCase>();
builder.Services.AddScoped<IUpdateOrderUseCase, UpdateOrderUseCase>();

builder.Services.AddLogging();

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddType<UserType>()
    .AddFiltering()
    .AddSorting()
    .AddProjections()
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true);

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGraphQL();

app.Run();