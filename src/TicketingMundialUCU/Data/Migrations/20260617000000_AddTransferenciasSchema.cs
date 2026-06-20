using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketingMundialUCU.Migrations;

/// <inheritdoc />
public partial class AddTransferenciasSchema : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            -- RF-55/RF-56: solicitudes de transferencia entre usuarios generales.
            CREATE TABLE transferencias (
                id_transferencia serial NOT NULL,
                id_entrada uuid NOT NULL,
                id_solicitante text NOT NULL,
                id_receptor text NOT NULL,
                estado varchar(16) NOT NULL DEFAULT 'pendiente',
                fecha_solicitud timestamp NOT NULL DEFAULT NOW(),
                CONSTRAINT pk_transferencias PRIMARY KEY (id_transferencia),
                CONSTRAINT fk_transferencias_entrada
                    FOREIGN KEY (id_entrada) REFERENCES entradas(id_entrada) ON DELETE RESTRICT,
                CONSTRAINT fk_transferencias_solicitante
                    FOREIGN KEY (id_solicitante) REFERENCES usuarios_generales(usuario_id) ON DELETE RESTRICT,
                CONSTRAINT fk_transferencias_receptor
                    FOREIGN KEY (id_receptor) REFERENCES usuarios_generales(usuario_id) ON DELETE RESTRICT,
                CONSTRAINT ck_transferencias_estado
                    CHECK (estado IN ('pendiente', 'aceptada', 'rechazada')),
                CONSTRAINT ck_transferencias_distintos_usuarios
                    CHECK (id_solicitante <> id_receptor)
            );

            CREATE INDEX ix_transferencias_solicitante
                ON transferencias(id_solicitante, fecha_solicitud DESC);
            CREATE INDEX ix_transferencias_receptor
                ON transferencias(id_receptor, fecha_solicitud DESC);
            CREATE INDEX ix_transferencias_entrada
                ON transferencias(id_entrada);

            CREATE UNIQUE INDEX ux_transferencias_entrada_pendiente
                ON transferencias(id_entrada)
                WHERE estado = 'pendiente';
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP TABLE IF EXISTS transferencias;
            """);
    }
}
