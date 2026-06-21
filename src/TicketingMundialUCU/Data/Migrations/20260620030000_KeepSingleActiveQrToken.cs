using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TicketingMundialUCU.Data;

#nullable disable

namespace TicketingMundialUCU.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260620030000_KeepSingleActiveQrToken")]
public partial class KeepSingleActiveQrToken : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DELETE FROM tokens_qr
            WHERE id_dispositivo IS NULL;

            CREATE UNIQUE INDEX ux_tokens_qr_entrada_activa
                ON tokens_qr(id_entrada)
                WHERE id_dispositivo IS NULL;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP INDEX IF EXISTS ux_tokens_qr_entrada_activa;
            """);
    }
}
