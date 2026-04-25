using Npgsql;
using proyecto_escuela.Data;
using proyecto_escuela.Models;

namespace proyecto_escuela.Repositories;

public class PaqueteRepository
{
    private readonly DbConnectionPool _pool;

    public PaqueteRepository(DbConnectionPool pool)
    {
        _pool = pool;
    }

    public async Task<string> RegistrarAsync(Paquete p)
    {
        // Generar id_unico aleatorio
        p.IdUnico = Guid.NewGuid().ToString("N")[..12].ToUpper();

        await using var conn = await _pool.OpenAsync();

        // Insertar paquete
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO paquetes (id_unico, remitente, destinatario, direccion, estado, peso, comentarios)
            VALUES (@idUnico, @remitente, @destinatario, @direccion, @estado, @peso, @comentarios)
            RETURNING id
            """;
        cmd.Parameters.AddWithValue("idUnico", p.IdUnico);
        cmd.Parameters.AddWithValue("remitente", p.Remitente);
        cmd.Parameters.AddWithValue("destinatario", p.Destinatario);
        cmd.Parameters.AddWithValue("direccion", p.Direccion);
        cmd.Parameters.AddWithValue("estado", p.Estado);
        cmd.Parameters.AddWithValue("peso", p.Peso);
        cmd.Parameters.AddWithValue("comentarios", (object?)p.Comentarios ?? DBNull.Value);

        var paqueteId = (int)(await cmd.ExecuteScalarAsync())!;

        // Registrar en historial
        await using var cmdHistorial = conn.CreateCommand();
        cmdHistorial.CommandText = """
            INSERT INTO historial_estados (paquete_id, estado, comentario)
            VALUES (@paqueteId, 'EN_ALMACEN', 'Ingreso al almacén')
            """;
        cmdHistorial.Parameters.AddWithValue("paqueteId", paqueteId);
        await cmdHistorial.ExecuteNonQueryAsync();

        return p.IdUnico;
    }

    public async Task<Paquete?> ObtenerPorIdUnicoAsync(string idUnico)
    {
        await using var conn = await _pool.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM paquetes WHERE id_unico = @idUnico";
        cmd.Parameters.AddWithValue("idUnico", idUnico);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return new Paquete
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            IdUnico = reader.GetString(reader.GetOrdinal("id_unico")),
            Remitente = reader.GetString(reader.GetOrdinal("remitente")),
            Destinatario = reader.GetString(reader.GetOrdinal("destinatario")),
            Direccion = reader.GetString(reader.GetOrdinal("direccion")),
            FechaIngreso = reader.GetDateTime(reader.GetOrdinal("fecha_ingreso")),
            Estado = reader.GetString(reader.GetOrdinal("estado")),
            Peso = reader.GetDecimal(reader.GetOrdinal("peso")),
            Comentarios = reader.IsDBNull(reader.GetOrdinal("comentarios")) ? null : reader.GetString(reader.GetOrdinal("comentarios"))
        };
    }
}