﻿using LIN.Cloud.PostgreSQL.Manager.Services.Data;
using LIN.Types.Cloud.PostgreSQL.Models;
using LIN.Types.Responses;
using Npgsql;

namespace LIN.Cloud.PostgreSQL.Manager.Services;

public class DatabasesManager
{

    private readonly NpgsqlConnection _dbConnection;
    private readonly DatabaseConector conector;

    public DatabasesManager(NpgsqlConnection dbConnection, DatabaseConector conector)
    {
        _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        this.conector = conector;
    }


    /// <summary>
    /// Crear base de datos.
    /// </summary>
    /// <param name="databaseName">Nombre de BD.</param>
    public async Task<CreateResponse> CreateDatabaseAsync(string databaseName)
    {
        try
        {
            var commandText = $"CREATE DATABASE {databaseName}";

            using var command = _dbConnection.CreateCommand();
            command.CommandText = commandText;
            await command.ExecuteNonQueryAsync();
            return new(Responses.Success);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("already exists"))
                return new()
                {
                    Response = Responses.ResourceExist,
                    Errors = [
                    new() {
                        Tittle = "Base de datos",
                        Description = $"El nombre de la base de datos {databaseName} esta ocupada",
                        Type = Types.Enumerations.ErrorTypes.User
                    }]
                };
        }

        return new(Responses.Undefined);
    }







    public async Task DeleteDatabaseAsync(string databaseName)
    {
        var commandText = $"DROP DATABASE IF EXISTS {databaseName}";
        using (var command = _dbConnection.CreateCommand())
        {
            command.CommandText = commandText;
            await command.ExecuteNonQueryAsync();
        }
    }

    public async Task UpdateDatabaseAsync(string oldName, string newName)
    {
        var commandText = $"ALTER DATABASE {oldName} RENAME TO {newName}";
        using (var command = _dbConnection.CreateCommand())
        {
            command.CommandText = commandText;
            await command.ExecuteNonQueryAsync();
        }
    }


    public async Task<object?> GetSize(string name)
    {
        var commandText = $"SELECT pg_size_pretty(pg_database_size('{name}'));";
        using var command = _dbConnection.CreateCommand();
        command.CommandText = commandText;
        var xx = await command.ExecuteScalarAsync();
        return xx;
    }


    public async Task<List<DataBaseModel>> ReadAll(int id)
    {

        conector.Start("master");

        var commandText = $"SELECT * FROM Databases where id_resource = {id}";

        List<DataBaseModel> strings = [];
        // Crear comando
        using (var command = new NpgsqlCommand(commandText, conector.Connection))
        {
            // Ejecutar el comando y obtener un lector
            using (var reader = command.ExecuteReader())
            {
                // Leer los resultados
                while (reader.Read())
                {
                    int idA = reader.GetInt32(0); // Leer la columna "Nombre"
                    string nombre = reader.GetString(1); // Leer la columna "Nombre"
                    int idUsuario = reader.GetInt32(2); // Leer la columna "id_resource"

                    strings.Add(new()
                    {
                        Id = idA,
                        Name = nombre,
                    });
                }
            }
        }

        return strings;
    }


    public async Task<DataBaseModel?> Read(int id)
    {

        conector.Start("master");

        var commandText = $"SELECT * FROM Databases where id_resource = {id}";

        List<DataBaseModel> strings = [];
        // Crear comando
        using (var command = new NpgsqlCommand(commandText, conector.Connection))
        {
            // Ejecutar el comando y obtener un lector
            using (var reader = command.ExecuteReader())
            {
                // Leer los resultados
                while (reader.Read())
                {
                    int idA = reader.GetInt32(0); // Leer la columna "Nombre"
                    string nombre = reader.GetString(1); // Leer la columna "Nombre"
                    int idUsuario = reader.GetInt32(2); // Leer la columna "id_resource"

                    strings.Add(new()
                    {
                        Id = idA,
                        Name = nombre,
                    });
                }
            }
        }

        return strings.FirstOrDefault();
    }


}