using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TicketingMundialUCU.Data;

#nullable disable

namespace TicketingMundialUCU.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260620040000_AddCodigoTokenToTokensQr")]
public partial class AddCodigoTokenToTokensQr : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            ALTER TABLE tokens_qr
                ADD COLUMN codigo_token uuid NULL;

            UPDATE tokens_qr
            SET codigo_token = id_token_qr
            WHERE codigo_token IS NULL;

            ALTER TABLE tokens_qr
                ALTER COLUMN codigo_token SET NOT NULL;

            CREATE UNIQUE INDEX ux_tokens_qr_codigo_token_activo
                ON tokens_qr(codigo_token)
                WHERE id_dispositivo IS NULL;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP INDEX IF EXISTS ux_tokens_qr_codigo_token_activo;

            ALTER TABLE tokens_qr
                DROP COLUMN IF EXISTS codigo_token;
            """);
    }
}
