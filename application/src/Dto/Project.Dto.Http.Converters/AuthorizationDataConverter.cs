using System.Diagnostics.CodeAnalysis;
using Project.Core.Models.User;
using Project.Dto.Http.User;

namespace Project.Dto.Http.Converters;

public class AuthorizationDataConverter
{
    [return: NotNullIfNotNull(nameof(authorizationData))]
    public static AuthorizationDataDto? Convert(AuthorizationData? authorizationData)
    {
        if (authorizationData is null)
            return null;

        return new AuthorizationDataDto(authorizationData.Email,
            authorizationData.Token
        );
    }
}