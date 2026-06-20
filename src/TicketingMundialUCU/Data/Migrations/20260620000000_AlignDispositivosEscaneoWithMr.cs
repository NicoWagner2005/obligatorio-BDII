using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TicketingMundialUCU.Data;

#nullable disable

namespace TicketingMundialUCU.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260620000000_AlignDispositivosEscaneoWithMr")]
public partial class AlignDispositivosEscaneoWithMr : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            ALTER TABLE validaciones_acceso
                DROP CONSTRAINT fk_validaciones_dispositivo;

            ALTER TABLE dispositivos_autorizados
                RENAME TO dispositivos_escaneo;

            ALTER TABLE dispositivos_escaneo
                RENAME CONSTRAINT fk_dispositivos_funcionario TO fk_dispositivos_escaneo_funcionario;

            ALTER TABLE dispositivos_escaneo
                RENAME COLUMN id_dispositivo TO id_dispositivo_num;

            ALTER TABLE dispositivos_escaneo
                RENAME COLUMN identificador TO id_dispositivo;

            ALTER TABLE dispositivos_escaneo
                DROP CONSTRAINT pk_dispositivos;

            ALTER TABLE dispositivos_escaneo
                DROP CONSTRAINT uq_dispositivos_identificador;

            ALTER TABLE validaciones_acceso
                ADD COLUMN id_dispositivo_nuevo varchar(64);

            UPDATE validaciones_acceso va
            SET id_dispositivo_nuevo = de.id_dispositivo
            FROM dispositivos_escaneo de
            WHERE va.id_dispositivo = de.id_dispositivo_num;

            ALTER TABLE validaciones_acceso
                DROP COLUMN id_dispositivo;

            ALTER TABLE validaciones_acceso
                RENAME COLUMN id_dispositivo_nuevo TO id_dispositivo;

            ALTER TABLE dispositivos_escaneo
                DROP COLUMN id_dispositivo_num;

            ALTER TABLE dispositivos_escaneo
                ADD CONSTRAINT pk_dispositivos_escaneo PRIMARY KEY (id_dispositivo);

            ALTER TABLE validaciones_acceso
                ADD CONSTRAINT fk_validaciones_dispositivo
                    FOREIGN KEY (id_dispositivo)
                    REFERENCES dispositivos_escaneo(id_dispositivo)
                    ON DELETE SET NULL;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            ALTER TABLE validaciones_acceso
                DROP CONSTRAINT fk_validaciones_dispositivo;

            ALTER TABLE dispositivos_escaneo
                DROP CONSTRAINT pk_dispositivos_escaneo;

            ALTER TABLE dispositivos_escaneo
                ADD COLUMN id_dispositivo_num int;

            WITH dispositivos_numerados AS (
                SELECT
                    id_dispositivo,
                    row_number() OVER (ORDER BY id_dispositivo)::int AS nuevo_id
                FROM dispositivos_escaneo
            )
            UPDATE dispositivos_escaneo de
            SET id_dispositivo_num = dn.nuevo_id
            FROM dispositivos_numerados dn
            WHERE de.id_dispositivo = dn.id_dispositivo;

            CREATE SEQUENCE IF NOT EXISTS dispositivos_autorizados_id_dispositivo_seq;

            SELECT setval(
                'dispositivos_autorizados_id_dispositivo_seq',
                COALESCE(MAX(id_dispositivo_num), 1),
                MAX(id_dispositivo_num) IS NOT NULL)
            FROM dispositivos_escaneo;

            ALTER TABLE dispositivos_escaneo
                ALTER COLUMN id_dispositivo_num SET DEFAULT nextval('dispositivos_autorizados_id_dispositivo_seq');

            ALTER SEQUENCE dispositivos_autorizados_id_dispositivo_seq
                OWNED BY dispositivos_escaneo.id_dispositivo_num;

            ALTER TABLE validaciones_acceso
                ADD COLUMN id_dispositivo_num int;

            UPDATE validaciones_acceso va
            SET id_dispositivo_num = de.id_dispositivo_num
            FROM dispositivos_escaneo de
            WHERE va.id_dispositivo = de.id_dispositivo;

            ALTER TABLE validaciones_acceso
                DROP COLUMN id_dispositivo;

            ALTER TABLE validaciones_acceso
                RENAME COLUMN id_dispositivo_num TO id_dispositivo;

            ALTER TABLE dispositivos_escaneo
                RENAME COLUMN id_dispositivo TO identificador;

            ALTER TABLE dispositivos_escaneo
                RENAME COLUMN id_dispositivo_num TO id_dispositivo;

            ALTER TABLE dispositivos_escaneo
                ALTER COLUMN id_dispositivo SET NOT NULL;

            ALTER TABLE dispositivos_escaneo
                ALTER COLUMN identificador SET NOT NULL;

            ALTER TABLE dispositivos_escaneo
                ADD CONSTRAINT pk_dispositivos PRIMARY KEY (id_dispositivo);

            ALTER TABLE dispositivos_escaneo
                ADD CONSTRAINT uq_dispositivos_identificador UNIQUE (identificador);

            ALTER TABLE dispositivos_escaneo
                RENAME CONSTRAINT fk_dispositivos_escaneo_funcionario TO fk_dispositivos_funcionario;

            ALTER TABLE dispositivos_escaneo
                RENAME TO dispositivos_autorizados;

            ALTER TABLE validaciones_acceso
                ADD CONSTRAINT fk_validaciones_dispositivo
                    FOREIGN KEY (id_dispositivo)
                    REFERENCES dispositivos_autorizados(id_dispositivo)
                    ON DELETE SET NULL;
            """);
    }
}
