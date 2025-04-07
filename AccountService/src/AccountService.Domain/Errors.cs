using SharedKernel;

namespace AccountService.Domain;

public static class UserErrors
{
    public static ErrorList UserNameAlreadyExist()
    {
        return Error.Validation("username.already.exists", "Пользователь с таким именем пользователя уже существует.");
    }
}