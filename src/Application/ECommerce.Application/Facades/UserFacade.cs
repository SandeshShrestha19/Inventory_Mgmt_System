using Ecommerce.Domain.Models;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Ports;

public class UserFacade : IUserFacade
{
  private readonly IRegisterUserUseCase _registerUserUseCase;
  private readonly IUpdateUserUseCase _updateUserUseCase;
  private readonly IDeleteUserUseCase _deleteUserUseCase;
  private readonly IGetUserQueryUseCase _getUserQueryUseCase;
  private readonly IUserResponseUseCase _getUserByIdUseCase;

  public UserFacade(IRegisterUserUseCase registerUserUseCase, IUpdateUserUseCase udpateUserUseCase, IDeleteUserUseCase deleteUserUseCase, IGetUserQueryUseCase getUserQueryUseCase, IUserResponseUseCase userResponseUseCase)
  {
    _registerUserUseCase = registerUserUseCase;
    _updateUserUseCase = udpateUserUseCase;
    _deleteUserUseCase = deleteUserUseCase;
    _getUserQueryUseCase = getUserQueryUseCase;
    _getUserByIdUseCase = userResponseUseCase;
  }


  public async Task<User> AddAsync(AddUserModel addModel)
  {
    return await _registerUserUseCase.ExecuteAsync(addModel);
  }

  public async Task<bool> DeleteAsync(Guid id)
  {
    return await _deleteUserUseCase.DeleteAsync(id);
  }

  public IQueryable<UserResponseModel> GetAll(Guid? cursorId, int pageSize)
  {
    return _getUserQueryUseCase.GetAll(cursorId, pageSize);
  }

  public async Task<UserResponseModel> GetByIdAsync(Guid id)
  {
    return await _getUserByIdUseCase.GetByIdAsync(id);
  }

  public async Task UpdateAsync(Guid id, UpdateUserModel updateModel)
  {
    await _updateUserUseCase.ExecuteAsync(id, updateModel);
  }
}