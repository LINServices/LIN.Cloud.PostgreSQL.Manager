using Npgsql;

namespace LIN.Cloud.PostgreSQL.Manager.Services.Data;

public class DatabaseConector
{

    public NpgsqlConnection Connection { get; set; }

    private readonly string _databaseName;

    public DatabaseConector(IConfiguration configuration)
    {
        _databaseName = configuration.GetConnectionString("postgre");
    }

    public void Start(string databaseName)
    {
        Connection = new NpgsqlConnection($"{_databaseName} database={databaseName}");
        Connection.Open();
    }

}