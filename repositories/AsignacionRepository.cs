using Npgsql;
using proyecto_escuela.Data;
using proyecto_escuela.Models;

namespace proyecto_escuela.Repositories;

public class AsignacionRepository
{
    private readonly DbConnectionPool _pool;

    public AsignacionRepository(DbConnectionPool pool)
    {
        _pool = pool;
    }

    public async Task<List<Paquete>> ObtenerPaquetesEnAlmacenAsync()
    {
        var lista = new List<Paquete>();
        await using var conn = await _pool.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM paquetes WHERE estado = 'EN_ALMACEN' ORDER BY fecha_ingreso ASC";

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lista.Add(new Paquete
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                IdUnico = reader.GetString(reader.GetOrdinal("id_unico")),
                Remitente = reader.GetString(reader.GetOrdinal("remitente")),
                Destinatario = reader.GetString(reader.GetOrdinal("destinatario")),
                Direccion = reader.GetString(reader.GetOrdinal("direccion")),
                Peso = reader.GetDecimal(reader.GetOrdinal("peso")),
                Estado = reader.GetString(reader.GetOrdinal("estado"))
            });
        }
        return lista;
    }

    public async Task<List<Repartidor>> ObtenerRepartidoresActivosAsync()
    {
        var lista = new List<Repartidor>();
        await using var conn = await _pool.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM repartidores WHERE activo = true ORDER BY nombre ASC";

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

    public async Task AsignarAsync(int paqueteId, int repartidorId)
    {
        await using var conn = await _pool.OpenAsync();

        // Insertar asignacion
        await using var cmd1 = conn.CreateCommand();
        cmd1.CommandText = """
            INSERT INTO asignaciones (paquete_id, repartidor_id)
            VALUES (@paqueteId, @repartidorId)
            """;
        cmd1.Parameters.AddWithValue("paqueteId", paqueteId);
        cmd1.Parameters.AddWithValue("repartidorId", repartidorId);
        await cmd1.ExecuteNonQueryAsync();

        // Cambiar estado del paquete
        await using var cmd2 = conn.CreateCommand();
        cmd2.CommandText = """
            UPDATE paquetes SET estado = 'EN_RUTA' WHERE id = @paqueteId
            """;
        cmd2.Parameters.AddWithValue("paqueteId", paqueteId);
        await cmd2.ExecuteNonQueryAsync();

        // Registrar en historial
        await using var cmd3 = conn.CreateCommand();
        cmd3.CommandText = """
            INSERT INTO historial_estados (paquete_id, estado, comentario)
            VALUES (@paqueteId, 'EN_RUTA', 'Asignado a repartidor')
            """;
        cmd3.Parameters.AddWithValue("paqueteId", paqueteId);
        await cmd3.ExecuteNonQueryAsync();
    }
}