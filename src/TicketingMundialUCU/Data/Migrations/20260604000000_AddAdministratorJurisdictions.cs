using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TicketingMundialUCU.Data;

#nullable disable

namespace TicketingMundialUCU.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260604000000_AddAdministratorJurisdictions")]
public partial class AddAdministratorJurisdictions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            INSERT INTO paises_sede (nombre)
            VALUES ('Canadá'), ('México'), ('Estados Unidos')
            ON CONFLICT (nombre) DO NOTHING;

            CREATE TABLE administrador_asignado_a_pais_sede (
                id_administrador text NOT NULL,
                nombre_pais_sede varchar(64) NOT NULL,
                CONSTRAINT pk_administrador_asignado_a_pais_sede
                    PRIMARY KEY (id_administrador, nombre_pais_sede),
                CONSTRAINT uq_administrador_asignado_a_pais_sede_administrador
                    UNIQUE (id_administrador),
                CONSTRAINT fk_aaps_administradores
                    FOREIGN KEY (id_administrador)
                    REFERENCES administradores(usuario_id)
                    ON DELETE CASCADE,
                CONSTRAINT fk_aaps_paises_sede
                    FOREIGN KEY (nombre_pais_sede)
                    REFERENCES paises_sede(nombre)
                    ON DELETE RESTRICT
            );
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP TABLE IF EXISTS administrador_asignado_a_pais_sede;
            DELETE FROM paises_sede
            WHERE nombre IN ('Canadá', 'México', 'Estados Unidos')
              AND NOT EXISTS (
                  SELECT 1
                  FROM estadios
                  WHERE estadios.nombre_pais_sede = paises_sede.nombre
              );
            """);
    }
}
