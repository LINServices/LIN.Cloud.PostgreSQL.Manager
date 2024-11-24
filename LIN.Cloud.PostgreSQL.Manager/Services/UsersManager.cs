using Npgsql;

namespace LIN.Cloud.PostgreSQL.Manager.Services;

public class UsersManager(NpgsqlConnection dbConnection)
{

    /// <summary>
    /// Crear usuario.
    /// </summary>
    /// <param name="username">Usuario</param>
    /// <param name="password">Contraseña.</param>
    /// <param name="databaseName">Base de datos para dar permisos.</param>
    public async Task CreateUserAsync(string username, string password, string databaseName)
    {
        var commandText = $"CREATE USER \"{username}\" WITH PASSWORD '{password}'";

        using (var command = dbConnection.CreateCommand())
        {
            command.CommandText = commandText;
            await command.ExecuteNonQueryAsync();
        }

        await GrantPermissionsAsync(username, databaseName, "CONNECT");  // Permitir la conexión por defecto.
    }


    /// <summary>
    /// Dar permisos a un usuario sobre una BD.
    /// </summary>
    /// <param name="username">Usuario.</param>
    /// <param name="databaseName">Base de datos.</param>
    /// <param name="permissions">Permisos.</param>
    public async Task GrantPermissionsAsync(string username, string databaseName, string permissions)
    {
        var commandText = $"GRANT {permissions} ON DATABASE {databaseName} TO \"{username}\"";

        using (var command = dbConnection.CreateCommand())
        {
            command.CommandText = commandText;
            await command.ExecuteNonQueryAsync();
        }

    }


    /// <summary>
    /// Revocar un permiso a un usuario sobre una BD.
    /// </summary>
    /// <param name="username">Usuario.</param>
    /// <param name="databaseName">Base de datos.</param>
    /// <param name="permissions">Permisos.</param>
    public async Task RevokePermissionsAsync(string username, string databaseName, string permissions)
    {
        var commandText = $"REVOKE {permissions} ON DATABASE {databaseName} FROM \"{username}\"";
        using (var command = dbConnection.CreateCommand())
        {
            command.CommandText = commandText;
            await command.ExecuteNonQueryAsync();
        }
    }

}