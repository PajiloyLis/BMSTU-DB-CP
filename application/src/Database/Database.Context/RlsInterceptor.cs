using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Database.Context;

public class RlsInterceptor : DbConnectionInterceptor
{
    private readonly string _userId;

    public RlsInterceptor(string userId)
    {
        _userId = userId;
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = $"SET app.current_user_name = '{_userId}';";
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}