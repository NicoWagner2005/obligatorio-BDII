using Dapper;
using Npgsql;

namespace TicketingMundialUCU.Data.Repositories;

public class UserPhoneRepository(IConfiguration configuration)
{
    private readonly string _connectionString =
        configuration.GetConnectionString("DefaultConnection")!;

    public async Task AddAsync(string userId, string telefono)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        const string sql = """
            INSERT INTO "telefonos_usuario" ("usuario_id", "telefono")
            VALUES (@UserId, @Telefono);
            """;

        await connection.ExecuteAsync(sql, new
        {
            UserId = userId,
            Telefono = telefono
        });
    }

    public async Task ReplaceAsync(string userId, string? telefono)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        const string deleteSql = """
            DELETE FROM "telefonos_usuario"
            WHERE "usuario_id" = @UserId;
            """;

        await connection.ExecuteAsync(deleteSql, new { UserId = userId }, transaction);

        if (!string.IsNullOrWhiteSpace(telefono))
        {
            const string insertSql = """
                INSERT INTO "telefonos_usuario" ("usuario_id", "telefono")
                VALUES (@UserId, @Telefono);
                """;

            await connection.ExecuteAsync(insertSql, new
            {
                UserId = userId,
                Telefono = telefono
            }, transaction);
        }

        await transaction.CommitAsync();
    }

    public async Task<IEnumerable<string>> GetByUserIdAsync(string userId)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        const string sql = """
            SELECT "telefono"
            FROM "telefonos_usuario"
            WHERE "usuario_id" = @UserId
            ORDER BY "telefono";
            """;

        return await connection.QueryAsync<string>(sql, new { UserId = userId });
    }

    public async Task DeleteAsync(string userId, string telefono)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        const string sql = """
            DELETE FROM "telefonos_usuario"
            WHERE "usuario_id" = @UserId AND "telefono" = @Telefono;
            """;

        await connection.ExecuteAsync(sql, new
        {
            UserId = userId,
            Telefono = telefono
        });
    }
}
