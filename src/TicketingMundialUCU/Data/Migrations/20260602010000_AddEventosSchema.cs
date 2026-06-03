using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketingMundialUCU.Migrations
{
    /// <inheritdoc />
    public partial class AddEventosSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE equipos (
                    id_equipo serial NOT NULL,
                    nombre varchar(128) NOT NULL,
                    CONSTRAINT pk_equipos PRIMARY KEY (id_equipo),
                    CONSTRAINT uq_equipos_nombre UNIQUE (nombre)
                );

                CREATE TABLE eventos (
                    id_evento serial NOT NULL,
                    fecha_hora timestamp NOT NULL,
                    id_administrador text NOT NULL,
                    id_estadio int NOT NULL,
                    CONSTRAINT pk_eventos PRIMARY KEY (id_evento),
                    CONSTRAINT fk_eventos_administradores
                        FOREIGN KEY (id_administrador)
                        REFERENCES administradores(usuario_id)
                        ON DELETE RESTRICT,
                    CONSTRAINT fk_eventos_estadios
                        FOREIGN KEY (id_estadio)
                        REFERENCES estadios(id_estadio)
                        ON DELETE RESTRICT
                );

                -- Indice para la verificacion de superposicion (RF-31)
                CREATE INDEX ix_eventos_estadio_fecha ON eventos(id_estadio, fecha_hora);

                CREATE TABLE equipo_juega_evento (
                    id_equipo int NOT NULL,
                    id_evento int NOT NULL,
                    rol varchar(16) NOT NULL,
                    CONSTRAINT pk_equipo_juega_evento PRIMARY KEY (id_equipo, id_evento),
                    CONSTRAINT fk_eje_equipos
                        FOREIGN KEY (id_equipo) REFERENCES equipos(id_equipo)
                        ON DELETE RESTRICT,
                    CONSTRAINT fk_eje_eventos
                        FOREIGN KEY (id_evento) REFERENCES eventos(id_evento)
                        ON DELETE CASCADE,
                    CONSTRAINT ck_eje_rol CHECK (rol IN ('local', 'visitante')),
                    -- Un solo equipo local y uno visitante por evento
                    CONSTRAINT uq_eje_evento_rol UNIQUE (id_evento, rol)
                );

                -- RF-32/RF-33: sectores habilitados por evento.
                -- La FK desde detalle_venta hacia esta tabla impide vender entradas
                -- para sectores no habilitados (RF-33).
                CREATE TABLE evento_habilita_sector (
                    id_evento int NOT NULL,
                    id_estadio int NOT NULL,
                    id_sector varchar(2) NOT NULL,
                    CONSTRAINT pk_evento_habilita_sector
                        PRIMARY KEY (id_evento, id_estadio, id_sector),
                    CONSTRAINT fk_ehs_eventos
                        FOREIGN KEY (id_evento) REFERENCES eventos(id_evento)
                        ON DELETE CASCADE,
                    CONSTRAINT fk_ehs_sectores
                        FOREIGN KEY (id_estadio, id_sector) REFERENCES sectores(id_estadio, id_sector)
                        ON DELETE RESTRICT
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TABLE IF EXISTS evento_habilita_sector;
                DROP TABLE IF EXISTS equipo_juega_evento;
                DROP TABLE IF EXISTS eventos;
                DROP TABLE IF EXISTS equipos;
                """);
        }
    }
}
