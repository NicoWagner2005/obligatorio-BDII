using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TicketingMundialUCU.Data;

#nullable disable

namespace TicketingMundialUCU.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260617010000_FixDispositivoFkSetNull")]
public partial class FixDispositivoFkSetNull : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""SELECT 1;""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""SELECT 1;""");
    }
}
