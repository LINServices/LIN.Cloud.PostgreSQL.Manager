using LIN.Types.Responses;
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
    public async Task<CreateResponse> CreateUserAsync(string username, string password, string databaseName)
    {
        try
        {
            var commandText = $"CREATE USER \"{username}\" WITH PASSWORD '{password.Replace("'", "''")}'";

            using (var command = dbConnection.CreateCommand())
            {
                command.CommandText = commandText;
                await command.ExecuteNonQueryAsync();
            }

            await GrantPermissionsAsync(username, databaseName, "CONNECT");  // Permitir la conexión por defecto.

            return new(Responses.Success);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("already exists"))
                return new(Responses.ExistAccount)
                {
                    Errors = [
                    new() {
                        Tittle = "Usuario de BD",
                        Description = $"El usuario {username} ya se encuentra en el sistema."
                    }]
                };
        }
        return new(Responses.Undefined);
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