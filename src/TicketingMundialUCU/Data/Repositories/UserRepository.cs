using Dapper;
using Npgsql;

namespace TicketingMundialUCU.Data.Repositories;

public class UserRepository(IConfiguration configuration)
{
    private readonly string _connectionString =
        configuration.GetConnectionString("DefaultConnection")!;

    public async Task CreateAsync(
        string email,
        string nroDocumento,
        string tipoDocumento,
        string paisDocumento,
        string paisDireccion,
        string localidad,
        string calle,
        string nroDireccion,
        string codigoPostal)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        const string sql = """
            INSERT INTO "usuarios"
                ("email", "nro_documento", "tipo_documento", "pais_documento", "pais_direccion", "localidad", "calle", "nro_direccion", "codigo_postal")
            VALUES
                (@Email, @NroDocumento, @TipoDocumento, @PaisDocumento, @PaisDireccion, @Localidad, @Calle, @NroDireccion, @CodigoPostal);
            """;

        await connection.ExecuteAsync(sql, new
        {
            Email = email,
            NroDocumento = nroDocumento,
            TipoDocumento = tipoDocumento,
            PaisDocumento = paisDocumento,
            PaisDireccion = paisDireccion,
            Localidad = localidad,
            Calle = calle,
            NroDireccion = nroDireccion,
            CodigoPostal = codigoPostal
        });
    }
}