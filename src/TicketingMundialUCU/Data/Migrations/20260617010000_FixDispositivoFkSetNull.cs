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
        migrationBuilder.Sql("""
            ALTER TABLE validaciones_acceso
                DROP CONSTRAINT fk_validaciones_dispositivo;

            ALTER TABLE validaciones_acceso
                ALTER COLUMN id_dispositivo DROP NOT NULL;

            ALTER TABLE validaciones_acceso
                ADD CONSTRAINT fk_validaciones_dispositivo
                    FOREIGN KEY (id_dispositivo)
                    REFERENCES dispositivos_autorizados(id_dispositivo)
                    ON DELETE SET NULL;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DELETE FROM validaciones_acceso WHERE id_dispositivo IS NULL;

            ALTER TABLE validaciones_acceso
                DROP CONSTRAINT fk_validaciones_dispositivo;

            ALTER TABLE validaciones_acceso
                ALTER COLUMN id_dispositivo SET NOT NULL;

            ALTER TABLE validaciones_acceso
                ADD CONSTRAINT fk_validaciones_dispositivo
                    FOREIGN KEY (id_dispositivo)
                    REFERENCES dispositivos_autorizados(id_dispositivo)
                    ON DELETE RESTRICT;
            """);
    }
}
