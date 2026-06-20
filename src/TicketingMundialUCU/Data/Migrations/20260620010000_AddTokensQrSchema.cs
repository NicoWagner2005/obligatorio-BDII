using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TicketingMundialUCU.Data;

#nullable disable

namespace TicketingMundialUCU.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260620010000_AddTokensQrSchema")]
public partial class AddTokensQrSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE tokens_qr (
                id_token_qr uuid NOT NULL,
                id_entrada uuid NOT NULL,
                fecha_expiracion timestamp NOT NULL,
                id_dispositivo varchar(64) NULL,
                CONSTRAINT pk_tokens_qr PRIMARY KEY (id_token_qr),
                CONSTRAINT fk_tokens_qr_entrada
                    FOREIGN KEY (id_entrada)
                    REFERENCES entradas(id_entrada)
                    ON DELETE CASCADE,
                CONSTRAINT fk_tokens_qr_dispositivo
                    FOREIGN KEY (id_dispositivo)
                    REFERENCES dispositivos_escaneo(id_dispositivo)
                    ON DELETE SET NULL
            );

            CREATE INDEX ix_tokens_qr_entrada
                ON tokens_qr(id_entrada);

            CREATE INDEX ix_tokens_qr_dispositivo
                ON tokens_qr(id_dispositivo);

            CREATE UNIQUE INDEX ux_tokens_qr_entrada_validada
                ON tokens_qr(id_entrada)
                WHERE id_dispositivo IS NOT NULL;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP TABLE IF EXISTS tokens_qr;
            """);
    }
}
