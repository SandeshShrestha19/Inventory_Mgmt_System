using BCrypt.Net;
using ECommerce.Domain.Models;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Ports;
using Microsoft.Extensions.Logging;
using Ecommerce.Domain.Models;

namespace ECommerce.Application.UseCases;

public class UpdateUserUseCase : IUpdateUserUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UpdateUserUseCase> _logger;

    public UpdateUserUseCase(IUserRepository userRepository, ILogger<UpdateUserUseCase> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<User> ExecuteAsync(Guid id, UpdateUserModel model)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id)
                ?? throw new Exception("User not found!");

            if (!string.IsNullOrWhiteSpace(model.Name))
                user.Name = model.Name;

            if (!string.IsNullOrWhiteSpace(model.Email))
                user.Email = model.Email;

            // ✅ only hash if password provided
            if (!string.IsNullOrWhiteSpace(model.Password))
                user.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);

            return await _userRepository.UpdateAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user details!");
            throw;
        }
    }
}