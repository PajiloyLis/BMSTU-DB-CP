using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;

namespace Database.Context;

public class TempViewConnectionInterceptor : DbConnectionInterceptor
{
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        if (connection is NpgsqlConnection npgsqlConn)
        {
            using var cmd = npgsqlConn.CreateCommand();
            cmd.CommandText = "create temporary view employee as select *, extract(year from age(employee_base.birth_date)) as age from employee_base;";
            cmd.ExecuteNonQuery();
        }
        base.ConnectionOpened(connection, eventData);
    }
}