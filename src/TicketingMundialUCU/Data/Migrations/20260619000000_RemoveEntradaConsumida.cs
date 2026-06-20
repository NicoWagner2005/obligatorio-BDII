using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketingMundialUCU.Migrations;

/// <inheritdoc />
public partial class RemoveEntradaConsumida : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            ALTER TABLE entradas DROP COLUMN IF EXISTS consumida;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            ALTER TABLE entradas ADD COLUMN IF NOT EXISTS consumida boolean NOT NULL DEFAULT FALSE;

            UPDATE entradas en
            SET consumida = TRUE
            WHERE EXISTS (
                SELECT 1
                FROM validaciones_acceso va
                WHERE va.id_entrada = en.id_entrada
            );
            """);
    }
}
