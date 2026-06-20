using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketingMundialUCU.Migrations;

/// <inheritdoc />
public partial class RemoveEntradaConsumida : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""SELECT 1;""");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""SELECT 1;""");
    }
}
