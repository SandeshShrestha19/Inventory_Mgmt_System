using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Ports;

public interface IGetUserQueryUseCase
{
  IQueryable<UserResponseModel> GetAll(Guid? cursor, int pageSize);
}