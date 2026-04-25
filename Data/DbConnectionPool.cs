using Npgsql;

namespace proyecto_escuela.Data;

public class DbConnectionPool
{
    private readonly NpgsqlDataSource _dataSource;

    public DbConnectionPool(IConfiguration config)
    {
        var connStr = config.GetConnectionString("Postgres")
                     ?? throw new InvalidOperationException("Falta ConnectionStrings:Postgres en appsettings.json");

        _dataSource = NpgsqlDataSource.Create(connStr);
    }

    public async Task<NpgsqlConnection> OpenAsync()
        => await _dataSource.OpenConnectionAsync();

    public NpgsqlConnection Open()
        => _dataSource.OpenConnection();
}