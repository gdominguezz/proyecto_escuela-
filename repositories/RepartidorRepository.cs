using Npgsql;
using proyecto_escuela.Data;
using proyecto_escuela.Models;

namespace proyecto_escuela.Repositories;

public class RepartidorRepository
{
    private readonly DbConnectionPool _pool;

    public RepartidorRepository(DbConnectionPool pool)
    {
        _pool = pool;
    }

    public async Task<List<Repartidor>> ObtenerTodosAsync()
    {
        var lista = new List<Repartidor>();
        await using var conn = await _pool.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM repartidores ORDER BY nombre ASC";

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lista.Add(new Repartidor
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Nombre = reader.GetString(reader.GetOrdinal("nombre")),
                Telefono = reader.GetString(reader.GetOrdinal("telefono")),
                Activo = reader.GetBoolean(reader.GetOrdinal("activo"))
            });
        }
        return lista;
    }

    public async Task AgregarAsync(Repartidor r)
    {
        await using var conn = await _pool.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO repartidores (nombre, telefono, activo)
            VALUES (@nombre, @telefono, true)
            """;
        cmd.Parameters.AddWithValue("nombre", r.Nombre);
        cmd.Parameters.AddWithValue("telefono", r.Telefono);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task ToggleActivoAsync(int id)
    {
        await using var conn = await _pool.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE repartidores SET activo = NOT activo WHERE id = @id";
        cmd.Parameters.AddWithValue("id", id);
        await cmd.ExecuteNonQueryAsync();
    }
}