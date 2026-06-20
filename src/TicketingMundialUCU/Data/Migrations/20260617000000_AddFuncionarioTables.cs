using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TicketingMundialUCU.Data;

#nullable disable

namespace TicketingMundialUCU.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260617000000_AddFuncionarioTables")]
public partial class AddFuncionarioTables : Migration
{
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                -- RF-71/RF-72/RF-73: dispositivos de escaneo autorizados, vinculados a un funcionario
                CREATE TABLE dispositivos_escaneo (
                    id_dispositivo varchar(64) NOT NULL,
                    id_funcionario text NOT NULL,
                    CONSTRAINT pk_dispositivos_escaneo PRIMARY KEY (id_dispositivo),
                    CONSTRAINT fk_dispositivos_escaneo_funcionario
                        FOREIGN KEY (id_funcionario)
                        REFERENCES funcionarios(usuario_id)
                        ON DELETE CASCADE
                );

                -- RF-75/RF-76: asignación de funcionarios a sectores por evento
                CREATE TABLE funcionario_sector_evento (
                    id_funcionario text NOT NULL,
                    id_evento int NOT NULL,
                    id_estadio int NOT NULL,
                    id_sector varchar(2) NOT NULL,
                    CONSTRAINT pk_funcionario_sector_evento
                        PRIMARY KEY (id_funcionario, id_evento, id_estadio, id_sector),
                    CONSTRAINT fk_fse_funcionario
                        FOREIGN KEY (id_funcionario)
                        REFERENCES funcionarios(usuario_id)
                        ON DELETE CASCADE,
                    CONSTRAINT fk_fse_sector
                        FOREIGN KEY (id_evento, id_estadio, id_sector)
                        REFERENCES evento_habilita_sector(id_evento, id_estadio, id_sector)
                        ON DELETE CASCADE
                );
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TABLE IF EXISTS funcionario_sector_evento;
                DROP TABLE IF EXISTS dispositivos_escaneo;
                """);
        }
}
