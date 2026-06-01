using Dapper;
using Npgsql;

namespace TicketingMundialUCU.Data.Repositories;

public class UserPhoneRepository(IConfiguration configuration)
{
    private readonly string _connectionString =
        configuration.GetConnectionString("DefaultConnection")!;

    public async Task AddAsync(string email, string telefono)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        const string sql = """
            INSERT INTO "telefonos_usuario" ("email", "telefono")
            VALUES (@Email, @Telefono);
            """;

        await connection.ExecuteAsync(sql, new
        {
            Email = email,
            Telefono = telefono
        });
    }

    public async Task<IEnumerable<string>> GetByUserEmailAsync(string email)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        const string sql = """
            SELECT "telefono"
            FROM "telefonos_usuario"
            WHERE "email" = @Email
            ORDER BY "telefono";
            """;

        return await connection.QueryAsync<string>(sql, new { Email = email });
    }

    public async Task DeleteAsync(string email, string telefono)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        const string sql = """
            DELETE FROM "telefonos_usuario"
            WHERE "email" = @Email AND "telefono" = @Telefono;
            """;

        await connection.ExecuteAsync(sql, new
        {
            Email = email,
            Telefono = telefono
        });
    }
}
