using LIN.Access.Developer.Controllers;
using LIN.Cloud.PostgreSQL.Manager.Services.Data;

namespace LIN.Cloud.PostgreSQL.Manager.Services.Local;

public class ManagementService(DatabasesManager databaseManager, UsersManager usersManager, DatabaseConector conector)
{

    /// <summary>
    /// Crear base de datos con usuario.
    /// </summary>
    /// <param name="databaseName">Nombre BD.</param>
    /// <param name="username">Usuario.</param>
    /// <param name="password">Contraseña.</param>
    public async Task CreateDatabaseWithUserAsync(string databaseName, string username, string password, int project)
    {
        // Crear la base de datos
        await databaseManager.CreateDatabaseAsync(databaseName);

        // Crear el usuario
        await usersManager.CreateUserAsync(username, password, databaseName);

        // Crear registro
        conector.Start("master");

        var commandText = $"INSERT INTO Databases (nombre, id_resource) VALUES ('{databaseName}', {project})";
        using (var command = conector.Connection.CreateCommand())
        {
            command.CommandText = commandText;
            await command.ExecuteNonQueryAsync();
        }


    }

    public async Task DeleteDatabaseWithUserAsync(string databaseName, string username)
    {
        // Revocar permisos antes de eliminar la base de datos
        await usersManager.RevokePermissionsAsync(username, databaseName, "ALL");

        // Eliminar la base de datos
        await databaseManager.DeleteDatabaseAsync(databaseName);

    }
}