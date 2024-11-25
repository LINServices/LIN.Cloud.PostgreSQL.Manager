using LIN.Cloud.PostgreSQL.Manager.Services.Data;
using LIN.Types.Responses;

namespace LIN.Cloud.PostgreSQL.Manager.Services.Local;

public class ManagementService(DatabasesManager databaseManager, UsersManager usersManager, DatabaseConector conector)
{

    /// <summary>
    /// Crear base de datos con usuario.
    /// </summary>
    /// <param name="databaseName">Nombre BD.</param>
    /// <param name="username">Usuario.</param>
    /// <param name="password">Contraseña.</param>
    public async Task<CreateResponse> CreateDatabaseWithUserAsync(string databaseName, string username, string password, int project)
    {
        // Crear la base de datos
        var databaseResponse = await databaseManager.CreateDatabaseAsync(databaseName);

        if (databaseResponse.Response != Responses.Success)
            return databaseResponse;

        // Crear el usuario
        var userResponse = await usersManager.CreateUserAsync(username, password, databaseName);

        if (userResponse.Response != Responses.Success)
        {
            // Eliminar BD.
            await databaseManager.DeleteDatabaseAsync(databaseName);
            return userResponse;
        }


        // Crear registro
        conector.Start("master");

        var commandText = $"INSERT INTO Databases (nombre, id_resource) VALUES ('{databaseName}', {project})";
        using (var command = conector.Connection.CreateCommand())
        {
            command.CommandText = commandText;
            await command.ExecuteNonQueryAsync();
        }

        return new(Responses.Success);
    }

    public async Task DeleteDatabaseWithUserAsync(string databaseName, string username)
    {
        // Revocar permisos antes de eliminar la base de datos
        await usersManager.RevokePermissionsAsync(username, databaseName, "ALL");

        // Eliminar la base de datos
        await databaseManager.DeleteDatabaseAsync(databaseName);

    }
}