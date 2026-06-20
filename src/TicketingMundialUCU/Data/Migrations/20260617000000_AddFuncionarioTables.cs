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
                -- RF-65: marca si la entrada fue consumida en el evento
                ALTER TABLE entradas ADD COLUMN consumida boolean NOT NULL DEFAULT FALSE;

                -- RF-71/RF-72/RF-73: dispositivos de escaneo autorizados, vinculados a un funcionario
                CREATE TABLE dispositivos_autorizados (
                    id_dispositivo serial NOT NULL,
                    identificador varchar(64) NOT NULL,
                    id_funcionario text NOT NULL,
                    fecha_registro timestamp NOT NULL DEFAULT NOW(),
                    CONSTRAINT pk_dispositivos PRIMARY KEY (id_dispositivo),
                    CONSTRAINT uq_dispositivos_identificador UNIQUE (identificador),
                    CONSTRAINT fk_dispositivos_funcionario
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

                -- RF-74/RF-77: registro de validaciones de acceso
                CREATE TABLE validaciones_acceso (
                    id_validacion serial NOT NULL,
                    id_entrada uuid NOT NULL,
                    id_funcionario text NOT NULL,
                    id_dispositivo int NOT NULL,
                    fecha_hora_validacion timestamp NOT NULL DEFAULT NOW(),
                    CONSTRAINT pk_validaciones PRIMARY KEY (id_validacion),
                    CONSTRAINT uq_validaciones_entrada UNIQUE (id_entrada),
                    CONSTRAINT fk_validaciones_entrada
                        FOREIGN KEY (id_entrada)
                        REFERENCES entradas(id_entrada)
                        ON DELETE RESTRICT,
                    CONSTRAINT fk_validaciones_funcionario
                        FOREIGN KEY (id_funcionario)
                        REFERENCES funcionarios(usuario_id)
                        ON DELETE RESTRICT,
                    CONSTRAINT fk_validaciones_dispositivo
                        FOREIGN KEY (id_dispositivo)
                        REFERENCES dispositivos_autorizados(id_dispositivo)
                        ON DELETE RESTRICT
                );
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TABLE IF EXISTS validaciones_acceso;
                DROP TABLE IF EXISTS funcionario_sector_evento;
                DROP TABLE IF EXISTS dispositivos_autorizados;
                ALTER TABLE entradas DROP COLUMN IF EXISTS consumida;
                """);
        }
}
