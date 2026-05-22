using ECommerce.Domain.Entities;
using ECommerce.Domain.Ports;
using Microsoft.Extensions.Logging;

public class GetUserQueryUseCase : IGetUserQueryUseCase
{
  private readonly IUserRepository _userRepository;

  public GetUserQueryUseCase(IUserRepository userRepository)
  {
    _userRepository = userRepository;
  }
  public IQueryable<UserResponseModel> GetAll(Guid? cursorId, int pageSize)
  {
    var users = _userRepository.GetAllAsync();
    if (cursorId.HasValue)
    {
      users = users.Where(x => x.Id > cursorId.Value);
    }
    return users.OrderBy(x => x.Id)
    .Take(pageSize)
    .Select(x => new UserResponseModel
    {
      Id = x.Id,
      Name = x.Name,
      Email = x.Email,
      IsLoggedIn = x.IsLoggedIn,
      IsActive = x.IsActive,
      CreatedAt = x.CreatedAt,
      Orders = x.Orders
    });
  }
}