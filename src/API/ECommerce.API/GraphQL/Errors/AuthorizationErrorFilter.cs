using HotChocolate.Execution;

public class AuthorizationErrorFilter : IErrorFilter
{
    public IError OnError(IError error)
    {
        if (error.Code == "AUTH_NOT_AUTHORIZED")
        {
            return error.WithMessage("You do not have permission to access this resource.");
        }

        if (error.Code == "AUTH_NOT_AUTHENTICATED")
        {
            return error.WithMessage("You must be logged in to access this resource.");
        }

        if(error.Exception is BaseException baseException)
        {
            return error.WithMessage(baseException.Message)
                        .WithCode(baseException.ErrorCode);
        }

        return error;
    }
}