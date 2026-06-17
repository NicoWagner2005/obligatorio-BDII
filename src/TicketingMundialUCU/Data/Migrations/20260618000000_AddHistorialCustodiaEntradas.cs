using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketingMundialUCU.Migrations;

/// <inheritdoc />
public partial class AddHistorialCustodiaEntradas : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            -- RF-65: cadena de custodia de cada entrada.
            CREATE TABLE historial_custodia_entrada (
                id_movimiento serial NOT NULL,
                id_entrada uuid NOT NULL,
                tipo_movimiento varchar(16) NOT NULL,
                fecha_movimiento timestamp NOT NULL DEFAULT NOW(),
                id_transferencia int NULL,
                CONSTRAINT pk_historial_custodia_entrada PRIMARY KEY (id_movimiento),
                CONSTRAINT fk_hce_entrada
                    FOREIGN KEY (id_entrada) REFERENCES entradas(id_entrada) ON DELETE RESTRICT,
                CONSTRAINT fk_hce_transferencia
                    FOREIGN KEY (id_transferencia) REFERENCES transferencias(id_transferencia) ON DELETE RESTRICT,
                CONSTRAINT ck_hce_tipo_movimiento
                    CHECK (tipo_movimiento IN ('emision', 'transferencia')),
                CONSTRAINT ck_hce_transferencia_requerida
                    CHECK (
                        (tipo_movimiento = 'emision' AND id_transferencia IS NULL)
                        OR (tipo_movimiento = 'transferencia' AND id_transferencia IS NOT NULL)
                    )
            );

            CREATE INDEX ix_hce_entrada_fecha
                ON historial_custodia_entrada(id_entrada, fecha_movimiento, id_movimiento);

            CREATE INDEX ix_hce_entrada_tipo
                ON historial_custodia_entrada(id_entrada, tipo_movimiento);

            CREATE UNIQUE INDEX ux_hce_transferencia
                ON historial_custodia_entrada(id_transferencia)
                WHERE id_transferencia IS NOT NULL;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP TABLE IF EXISTS historial_custodia_entrada;
            """);
    }
}
