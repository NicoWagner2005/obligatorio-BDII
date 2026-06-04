using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketingMundialUCU.Migrations
{
    /// <inheritdoc />
    public partial class AddVentasSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                -- RF-34/RF-41: precio por sector habilitado en evento
                ALTER TABLE evento_habilita_sector ADD COLUMN precio numeric(10,2) NOT NULL DEFAULT 0;

                -- RF-42/RF-43: historial de tasas de comision
                CREATE TABLE tasa_comision (
                    id_tasa serial NOT NULL,
                    tasa numeric(5,4) NOT NULL,
                    fecha_desde timestamp NOT NULL,
                    CONSTRAINT pk_tasa_comision PRIMARY KEY (id_tasa),
                    CONSTRAINT ck_tasa_positiva CHECK (tasa > 0)
                );
                -- Tasa inicial del 5% (RF-42)
                INSERT INTO tasa_comision (tasa, fecha_desde) VALUES (0.0500, '2026-01-01 00:00:00');

                -- RF-38/RF-39/RF-40/RF-41/RF-44: ventas
                CREATE TABLE ventas (
                    id_venta serial NOT NULL,
                    id_usuario text NOT NULL,
                    fecha_venta timestamp NOT NULL DEFAULT NOW(),
                    estado varchar(16) NOT NULL DEFAULT 'pendiente',
                    monto_total numeric(12,2) NOT NULL,
                    tasa_comision_aplicada numeric(5,4) NOT NULL,
                    id_tasa int NOT NULL,
                    CONSTRAINT pk_ventas PRIMARY KEY (id_venta),
                    CONSTRAINT fk_ventas_usuario
                        FOREIGN KEY (id_usuario) REFERENCES "AspNetUsers"("Id") ON DELETE RESTRICT,
                    CONSTRAINT fk_ventas_tasa
                        FOREIGN KEY (id_tasa) REFERENCES tasa_comision(id_tasa) ON DELETE RESTRICT,
                    CONSTRAINT ck_ventas_estado
                        CHECK (estado IN ('pendiente', 'confirmada', 'paga'))
                );

                -- RF-35/RF-36/RF-45/RF-46: detalle de venta — una fila por entrada individual
                CREATE TABLE detalle_venta (
                    id_detalle serial NOT NULL,
                    id_venta int NOT NULL,
                    id_evento int NOT NULL,
                    id_estadio int NOT NULL,
                    id_sector varchar(2) NOT NULL,
                    precio_unitario numeric(10,2) NOT NULL,
                    codigo_entrada uuid NOT NULL DEFAULT gen_random_uuid(),
                    CONSTRAINT pk_detalle_venta PRIMARY KEY (id_detalle),
                    CONSTRAINT fk_dv_venta
                        FOREIGN KEY (id_venta) REFERENCES ventas(id_venta) ON DELETE CASCADE,
                    CONSTRAINT fk_dv_ehs
                        FOREIGN KEY (id_evento, id_estadio, id_sector)
                        REFERENCES evento_habilita_sector(id_evento, id_estadio, id_sector)
                        ON DELETE RESTRICT,
                    CONSTRAINT uq_dv_codigo UNIQUE (codigo_entrada)
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TABLE IF EXISTS detalle_venta;
                DROP TABLE IF EXISTS ventas;
                DROP TABLE IF EXISTS tasa_comision;
                ALTER TABLE evento_habilita_sector DROP COLUMN IF EXISTS precio;
                """);
        }
    }
}
