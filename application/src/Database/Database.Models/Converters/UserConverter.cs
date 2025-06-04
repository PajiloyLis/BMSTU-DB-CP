using System.Diagnostics.CodeAnalysis;
using Project.Core.Models.User;

namespace Database.Models.Converters;

public class UserConverter
{
    [return: NotNullIfNotNull(nameof(user))]
    public static BaseUser? Convert(UserDb? user)
    {
        if (user is null) return null;

        return new BaseUser(user.Email,
            user.Password,
            user.Salt,
            user.Role
            
        );
    }

    [return: NotNullIfNotNull(nameof(user))]
    public static UserDb? Convert(BaseUser? user)
    {
        if (user == null)
            return null;

        return new UserDb(user.Email,
            user.Password,
            user.Salt,
            user.Role
        );
    }
}