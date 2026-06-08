using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketingMundialUCU.Migrations
{
    /// <inheritdoc />
    public partial class AddEstadiosSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE paises_sede (
                    nombre varchar(64) NOT NULL,
                    CONSTRAINT pk_paises_sede PRIMARY KEY (nombre)
                );

                CREATE TABLE estadios (
                    id_estadio serial NOT NULL,
                    nombre varchar(128) NOT NULL,
                    nombre_pais_sede varchar(64) NOT NULL,
                    CONSTRAINT pk_estadios PRIMARY KEY (id_estadio),
                    CONSTRAINT fk_estadios_paises_sede
                        FOREIGN KEY (nombre_pais_sede)
                        REFERENCES paises_sede(nombre)
                        ON DELETE RESTRICT
                );

                CREATE TABLE sectores (
                    id_estadio int NOT NULL,
                    id_sector varchar(2) NOT NULL,
                    capacidad_max int NOT NULL,
                    CONSTRAINT pk_sectores PRIMARY KEY (id_estadio, id_sector),
                    CONSTRAINT fk_sectores_estadios
                        FOREIGN KEY (id_estadio)
                        REFERENCES estadios(id_estadio)
                        ON DELETE CASCADE,
                    CONSTRAINT ck_sectores_id
                        CHECK (id_sector IN ('A', 'B', 'C', 'D')),
                    CONSTRAINT ck_sectores_capacidad
                        CHECK (capacidad_max > 0)
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TABLE IF EXISTS sectores;
                DROP TABLE IF EXISTS estadios;
                DROP TABLE IF EXISTS paises_sede;
                """);
        }
    }
}
