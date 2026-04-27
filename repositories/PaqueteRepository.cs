using Npgsql;
using NpgsqlTypes;
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
        p.IdUnico = Guid.NewGuid().ToString("N")[..12].ToUpper();

        await using var conn = await _pool.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO paquetes (id_unico, remitente, destinatario, direccion, estado, peso, comentarios)
            VALUES (@idUnico, @remitente, @destinatario, @direccion, @estado, @peso, @comentarios)
            RETURNING id
            """;
        cmd.Parameters.Add(new NpgsqlParameter("idUnico", NpgsqlDbType.Text) { Value = p.IdUnico });
        cmd.Parameters.Add(new NpgsqlParameter("remitente", NpgsqlDbType.Text) { Value = p.Remitente });
        cmd.Parameters.Add(new NpgsqlParameter("destinatario", NpgsqlDbType.Text) { Value = p.Destinatario });
        cmd.Parameters.Add(new NpgsqlParameter("direccion", NpgsqlDbType.Text) { Value = p.Direccion });
        cmd.Parameters.Add(new NpgsqlParameter("estado", NpgsqlDbType.Text) { Value = p.Estado });
        cmd.Parameters.Add(new NpgsqlParameter("peso", NpgsqlDbType.Numeric) { Value = p.Peso });
        cmd.Parameters.Add(new NpgsqlParameter("comentarios", NpgsqlDbType.Text) { Value = (object?)p.Comentarios ?? DBNull.Value });

        var paqueteId = (int)(await cmd.ExecuteScalarAsync())!;

        await using var cmdHistorial = conn.CreateCommand();
        cmdHistorial.CommandText = """
            INSERT INTO historial_estados (paquete_id, estado, comentario)
            VALUES (@paqueteId, 'EN_ALMACEN', 'Ingreso al almacén')
            """;
        cmdHistorial.Parameters.Add(new NpgsqlParameter("paqueteId", NpgsqlDbType.Integer) { Value = paqueteId });
        await cmdHistorial.ExecuteNonQueryAsync();

        return p.IdUnico;
    }

    public async Task<Paquete?> ObtenerPorIdUnicoAsync(string idUnico)
    {
        await using var conn = await _pool.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM paquetes WHERE id_unico = @idUnico";
        cmd.Parameters.Add(new NpgsqlParameter("idUnico", NpgsqlDbType.Text) { Value = idUnico });

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

    public async Task<List<Paquete>> ObtenerTodosAsync()
    {
        var lista = new List<Paquete>();
        await using var conn = await _pool.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM paquetes ORDER BY fecha_ingreso DESC";

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
                FechaIngreso = reader.GetDateTime(reader.GetOrdinal("fecha_ingreso")),
                Estado = reader.GetString(reader.GetOrdinal("estado")),
                Peso = reader.GetDecimal(reader.GetOrdinal("peso")),
                Comentarios = reader.IsDBNull(reader.GetOrdinal("comentarios")) ? null : reader.GetString(reader.GetOrdinal("comentarios"))
            });
        }
        return lista;
    }

    public async Task<List<Paquete>> ObtenerEnRutaAsync()
    {
        var lista = new List<Paquete>();
        await using var conn = await _pool.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM paquetes WHERE estado = 'EN_RUTA' ORDER BY fecha_ingreso ASC";

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
                Estado = reader.GetString(reader.GetOrdinal("estado")),
                FechaIngreso = reader.GetDateTime(reader.GetOrdinal("fecha_ingreso"))
            });
        }
        return lista;
    }

    public async Task EntregarAsync(int paqueteId, string? comentario)
    {
        await using var conn = await _pool.OpenAsync();

        await using var cmd1 = conn.CreateCommand();
        cmd1.CommandText = "UPDATE paquetes SET estado = 'ENTREGADO' WHERE id = @id";
        cmd1.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Integer) { Value = paqueteId });
        await cmd1.ExecuteNonQueryAsync();

        await using var cmd2 = conn.CreateCommand();
        cmd2.CommandText = """
            INSERT INTO historial_estados (paquete_id, estado, comentario)
            VALUES (@id, 'ENTREGADO', @comentario)
            """;
        cmd2.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Integer) { Value = paqueteId });
        cmd2.Parameters.Add(new NpgsqlParameter("comentario", NpgsqlDbType.Text) { Value = (object?)comentario ?? DBNull.Value });
        await cmd2.ExecuteNonQueryAsync();
    }
}