using Ecommerce.Domain.Models;
using FluentValidation;

public class UserModelValidator : AbstractValidator<AddUserModel>
{
  public UserModelValidator()
  {
    RuleFor(x => x.Name)
    .NotEmpty().WithMessage("Name can't be empty!")
    .Length(3,50).WithMessage("Name must be of 3 to 50 characters!");

    RuleFor(x => x.Email)
    .NotEmpty().WithMessage("Email can't be empty!")
    .EmailAddress().WithMessage("Invalid email format!");

    RuleFor(x => x.Password)
    .NotEmpty().WithMessage("Password field can't be empty!")
    .Length(5,12).WithMessage("Password must be of 6 to 12 characters.");
  }
}