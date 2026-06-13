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
                    id_comprador text NOT NULL,
                    fecha_venta timestamp NOT NULL DEFAULT NOW(),
                    estado varchar(16) NOT NULL DEFAULT 'pendiente',
                    monto_total numeric(12,2) NOT NULL,
                    tasa_comision_aplicada numeric(5,4) NOT NULL,
                    id_tasa int NOT NULL,
                    CONSTRAINT pk_ventas PRIMARY KEY (id_venta),
                    CONSTRAINT fk_ventas_comprador
                        FOREIGN KEY (id_comprador) REFERENCES usuarios_generales(usuario_id) ON DELETE RESTRICT,
                    CONSTRAINT fk_ventas_tasa
                        FOREIGN KEY (id_tasa) REFERENCES tasa_comision(id_tasa) ON DELETE RESTRICT,
                    CONSTRAINT ck_ventas_estado
                        CHECK (estado IN ('pendiente', 'confirmada', 'paga'))
                );

                -- RF-35/RF-36: detalle de venta — una fila por item comprado
                CREATE TABLE detalle_venta (
                    id_venta int NOT NULL,
                    nro_linea int NOT NULL,
                    id_evento int NOT NULL,
                    id_estadio int NOT NULL,
                    id_sector varchar(2) NOT NULL,
                    cantidad int NOT NULL,
                    subtotal numeric(12,2) NOT NULL,
                    CONSTRAINT pk_detalle_venta PRIMARY KEY (id_venta, nro_linea),
                    CONSTRAINT fk_dv_venta
                        FOREIGN KEY (id_venta) REFERENCES ventas(id_venta) ON DELETE CASCADE,
                    CONSTRAINT fk_dv_ehs
                        FOREIGN KEY (id_evento, id_estadio, id_sector)
                        REFERENCES evento_habilita_sector(id_evento, id_estadio, id_sector)
                        ON DELETE RESTRICT,
                    CONSTRAINT ck_dv_cantidad CHECK (cantidad > 0 AND cantidad <= 5),
                    CONSTRAINT ck_dv_subtotal CHECK (subtotal > 0)
                );

                -- RF-45/RF-46/RF-47/RF-51: entradas individuales emitidas
                CREATE TABLE entradas (
                    id_entrada uuid NOT NULL,
                    id_poseedor text NOT NULL,
                    id_venta int NOT NULL,
                    nro_linea_detalle_venta int NOT NULL,
                    CONSTRAINT pk_entradas PRIMARY KEY (id_entrada),
                    CONSTRAINT fk_entradas_poseedor
                        FOREIGN KEY (id_poseedor) REFERENCES usuarios_generales(usuario_id) ON DELETE RESTRICT,
                    CONSTRAINT fk_entradas_detalle_venta
                        FOREIGN KEY (id_venta, nro_linea_detalle_venta)
                        REFERENCES detalle_venta(id_venta, nro_linea)
                        ON DELETE CASCADE
                );

                CREATE INDEX ix_entradas_poseedor ON entradas(id_poseedor);
                CREATE INDEX ix_entradas_detalle_venta ON entradas(id_venta, nro_linea_detalle_venta);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TABLE IF EXISTS entradas;
                DROP TABLE IF EXISTS detalle_venta;
                DROP TABLE IF EXISTS ventas;
                DROP TABLE IF EXISTS tasa_comision;
                ALTER TABLE evento_habilita_sector DROP COLUMN IF EXISTS precio;
                """);
        }
    }
}
