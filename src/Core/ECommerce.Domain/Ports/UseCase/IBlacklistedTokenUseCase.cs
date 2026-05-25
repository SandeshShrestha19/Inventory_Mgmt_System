public interface IBlacklistedTokenUseCase
{
  Task ExecuteAsync(AddBlacklistedTokenModel addModel);
}