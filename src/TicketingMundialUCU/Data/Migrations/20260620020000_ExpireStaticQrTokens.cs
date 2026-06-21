using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TicketingMundialUCU.Data;

#nullable disable

namespace TicketingMundialUCU.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260620020000_ExpireStaticQrTokens")]
public partial class ExpireStaticQrTokens : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE tokens_qr
            SET fecha_expiracion = LOCALTIMESTAMP
            WHERE id_dispositivo IS NULL
              AND fecha_expiracion > LOCALTIMESTAMP;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
