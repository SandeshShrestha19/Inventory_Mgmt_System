public interface IEnable2FAUseCase
{
  Task<Enable2FAResponseModel> ExecuteAsync(Guid id);
}