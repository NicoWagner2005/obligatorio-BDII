using Dapper;
using Npgsql;

namespace TicketingMundialUCU.Data.Repositories;

public class UserRepository(IConfiguration configuration) : IUserRepository
{
    private readonly string _connectionString =
        configuration.GetConnectionString("DefaultConnection")!;

    public async Task CreateAsync(
        string identityUserId,
        string nroDocumento,
        string tipoDocumento,
        string paisDocumento,
        string paisDireccion,
        string localidad,
        string calle,
        string nroDireccion,
        string codigoPostal,
        string role,
        string? paisSedeAsignado)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        const string userSql = """
            INSERT INTO "usuarios"
                ("id", "nro_documento", "tipo_documento", "pais_documento", "pais_direccion", "localidad", "calle", "nro_direccion", "codigo_postal")
            VALUES
                (@IdentityUserId, @NroDocumento, @TipoDocumento, @PaisDocumento, @PaisDireccion, @Localidad, @Calle, @NroDireccion, @CodigoPostal);
            """;

        await connection.ExecuteAsync(userSql, new
        {
            IdentityUserId = identityUserId,
            NroDocumento = nroDocumento,
            TipoDocumento = tipoDocumento,
            PaisDocumento = paisDocumento,
            PaisDireccion = paisDireccion,
            Localidad = localidad,
            Calle = calle,
            NroDireccion = nroDireccion,
            CodigoPostal = codigoPostal
        }, transaction);

        switch (role)
        {
            case "general":
                const string generalSql = """
                    INSERT INTO "usuarios_generales" ("usuario_id", "estado_identidad")
                    VALUES (@IdentityUserId, 'PENDIENTE');
                    """;
                await connection.ExecuteAsync(generalSql, new { IdentityUserId = identityUserId }, transaction);
                break;

            case "funcionario":
                const string funcionarioSql = """
                    INSERT INTO "funcionarios" ("usuario_id")
                    VALUES (@IdentityUserId);
                    """;
                await connection.ExecuteAsync(funcionarioSql, new { IdentityUserId = identityUserId }, transaction);
                break;

            case "administrador":
                const string administradorSql = """
                    INSERT INTO "administradores" ("usuario_id", "fecha_asignacion")
                    VALUES (@IdentityUserId, CURRENT_DATE);
                    """;
                await connection.ExecuteAsync(administradorSql, new { IdentityUserId = identityUserId }, transaction);

                const string jurisdictionSql = """
                    INSERT INTO administrador_asignado_a_pais_sede
                        (id_administrador, nombre_pais_sede)
                    VALUES (@IdentityUserId, @PaisSedeAsignado);
                    """;
                await connection.ExecuteAsync(
                    jurisdictionSql,
                    new { IdentityUserId = identityUserId, PaisSedeAsignado = paisSedeAsignado },
                    transaction);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(role), role, "Rol de usuario no soportado.");
        }

        await transaction.CommitAsync();
    }
}
