using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TicketingMundialUCU.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.Sql("""
            -- Esquema consolidado del dominio del obligatorio BDII.
            -- No incluye tablas AspNet* de Identity: esas pertenecen a la libreria de .NET.
            -- Este script asume que el esquema de Identity ya existe.


            CREATE TABLE IF NOT EXISTS usuarios (
                id text NOT NULL,
                nro_documento varchar(64) NOT NULL,
                tipo_documento varchar(16) NOT NULL,
                pais_documento varchar(32) NOT NULL,
                pais_direccion varchar(32) NOT NULL,
                localidad varchar(32) NOT NULL,
                calle varchar(32) NOT NULL,
                nro_direccion varchar(16) NOT NULL,
                codigo_postal varchar(16) NOT NULL,

                CONSTRAINT pk_usuarios PRIMARY KEY (id),
                CONSTRAINT fk_usuarios_id
                    FOREIGN KEY (id)
                    REFERENCES "AspNetUsers" ("Id")
                    ON DELETE CASCADE,
                CONSTRAINT uq_usuarios_documento
                    UNIQUE (nro_documento, tipo_documento, pais_documento)
            );

            CREATE TABLE IF NOT EXISTS usuarios_generales (
                usuario_id text NOT NULL,
                fecha_registro date NOT NULL DEFAULT CURRENT_DATE,
                estado_identidad varchar(16) NOT NULL,

                CONSTRAINT pk_usuarios_generales PRIMARY KEY (usuario_id),
                CONSTRAINT fk_usuarios_generales_usuarios
                    FOREIGN KEY (usuario_id)
                    REFERENCES usuarios(id)
                    ON DELETE CASCADE,
                CONSTRAINT ck_usuarios_generales_estado_identidad
                    CHECK (estado_identidad IN ('PENDIENTE', 'VERIFICADA', 'RECHAZADA'))
            );

            CREATE SEQUENCE IF NOT EXISTS funcionarios_legajo_seq;

            CREATE TABLE IF NOT EXISTS funcionarios (
                usuario_id text NOT NULL,
                nro_legajo varchar(16) NOT NULL
                    DEFAULT ('FUNC-' || lpad(nextval('funcionarios_legajo_seq')::text, 5, '0')),

                CONSTRAINT pk_funcionarios PRIMARY KEY (usuario_id),
                CONSTRAINT uq_funcionarios_nro_legajo UNIQUE (nro_legajo),
                CONSTRAINT fk_funcionarios_usuarios
                    FOREIGN KEY (usuario_id)
                    REFERENCES usuarios(id)
                    ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS administradores (
                usuario_id text NOT NULL,
                fecha_asignacion date NOT NULL,

                CONSTRAINT pk_administradores PRIMARY KEY (usuario_id),
                CONSTRAINT fk_administradores_usuarios
                    FOREIGN KEY (usuario_id)
                    REFERENCES usuarios(id)
                    ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS telefonos_usuario (
                usuario_id text NOT NULL,
                telefono varchar(20) NOT NULL,

                CONSTRAINT pk_telefonos_usuario PRIMARY KEY (usuario_id, telefono),
                CONSTRAINT fk_telefonos_usuario_usuarios
                    FOREIGN KEY (usuario_id)
                    REFERENCES usuarios(id)
                    ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS paises_sede (
                nombre varchar(64) NOT NULL,
                CONSTRAINT pk_paises_sede PRIMARY KEY (nombre)
            );

            CREATE TABLE IF NOT EXISTS estadios (
                id_estadio serial NOT NULL,
                nombre varchar(128) NOT NULL,
                nombre_pais_sede varchar(64) NOT NULL,

                CONSTRAINT pk_estadios PRIMARY KEY (id_estadio),
                CONSTRAINT fk_estadios_paises_sede
                    FOREIGN KEY (nombre_pais_sede)
                    REFERENCES paises_sede(nombre)
                    ON DELETE RESTRICT
            );

            CREATE TABLE IF NOT EXISTS sectores (
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

            CREATE TABLE IF NOT EXISTS administrador_asignado_a_pais_sede (
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

            CREATE TABLE IF NOT EXISTS equipos (
                id_equipo serial NOT NULL,
                nombre varchar(128) NOT NULL,

                CONSTRAINT pk_equipos PRIMARY KEY (id_equipo),
                CONSTRAINT uq_equipos_nombre UNIQUE (nombre)
            );

            CREATE TABLE IF NOT EXISTS eventos (
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

            CREATE TABLE IF NOT EXISTS equipo_juega_evento (
                id_equipo int NOT NULL,
                id_evento int NOT NULL,
                rol varchar(16) NOT NULL,

                CONSTRAINT pk_equipo_juega_evento PRIMARY KEY (id_equipo, id_evento),
                CONSTRAINT fk_eje_equipos
                    FOREIGN KEY (id_equipo)
                    REFERENCES equipos(id_equipo)
                    ON DELETE RESTRICT,
                CONSTRAINT fk_eje_eventos
                    FOREIGN KEY (id_evento)
                    REFERENCES eventos(id_evento)
                    ON DELETE CASCADE,
                CONSTRAINT ck_eje_rol
                    CHECK (rol IN ('local', 'visitante')),
                CONSTRAINT uq_eje_evento_rol
                    UNIQUE (id_evento, rol)
            );

            CREATE TABLE IF NOT EXISTS evento_habilita_sector (
                id_evento int NOT NULL,
                id_estadio int NOT NULL,
                id_sector varchar(2) NOT NULL,
                precio numeric(10,2) NOT NULL DEFAULT 0,

                CONSTRAINT pk_evento_habilita_sector
                    PRIMARY KEY (id_evento, id_estadio, id_sector),
                CONSTRAINT fk_ehs_eventos
                    FOREIGN KEY (id_evento)
                    REFERENCES eventos(id_evento)
                    ON DELETE CASCADE,
                CONSTRAINT fk_ehs_sectores
                    FOREIGN KEY (id_estadio, id_sector)
                    REFERENCES sectores(id_estadio, id_sector)
                    ON DELETE RESTRICT
            );

            CREATE TABLE IF NOT EXISTS tasa_comision (
                id_tasa serial NOT NULL,
                tasa numeric(5,4) NOT NULL,
                fecha_desde timestamp NOT NULL,

                CONSTRAINT pk_tasa_comision PRIMARY KEY (id_tasa),
                CONSTRAINT ck_tasa_positiva CHECK (tasa > 0)
            );

            CREATE TABLE IF NOT EXISTS ventas (
                id_venta serial NOT NULL,
                id_comprador text NOT NULL,
                fecha_venta timestamp NOT NULL DEFAULT NOW(),
                estado varchar(16) NOT NULL DEFAULT 'pendiente',
                monto_total numeric(12,2) NOT NULL,
                tasa_comision_aplicada numeric(5,4) NOT NULL,
                id_tasa int NOT NULL,

                CONSTRAINT pk_ventas PRIMARY KEY (id_venta),
                CONSTRAINT fk_ventas_comprador
                    FOREIGN KEY (id_comprador)
                    REFERENCES usuarios_generales(usuario_id)
                    ON DELETE RESTRICT,
                CONSTRAINT fk_ventas_tasa
                    FOREIGN KEY (id_tasa)
                    REFERENCES tasa_comision(id_tasa)
                    ON DELETE RESTRICT,
                CONSTRAINT ck_ventas_estado
                    CHECK (estado IN ('pendiente', 'confirmada', 'paga'))
            );

            CREATE TABLE IF NOT EXISTS detalle_venta (
                id_venta int NOT NULL,
                nro_linea int NOT NULL,
                id_evento int NOT NULL,
                id_estadio int NOT NULL,
                id_sector varchar(2) NOT NULL,
                cantidad int NOT NULL,
                subtotal numeric(12,2) NOT NULL,

                CONSTRAINT pk_detalle_venta PRIMARY KEY (id_venta, nro_linea),
                CONSTRAINT fk_dv_venta
                    FOREIGN KEY (id_venta)
                    REFERENCES ventas(id_venta)
                    ON DELETE CASCADE,
                CONSTRAINT fk_dv_ehs
                    FOREIGN KEY (id_evento, id_estadio, id_sector)
                    REFERENCES evento_habilita_sector(id_evento, id_estadio, id_sector)
                    ON DELETE RESTRICT,
                CONSTRAINT ck_dv_cantidad
                    CHECK (cantidad > 0 AND cantidad <= 5),
                CONSTRAINT ck_dv_subtotal
                    CHECK (subtotal > 0)
            );

            CREATE TABLE IF NOT EXISTS entradas (
                id_entrada uuid NOT NULL,
                id_poseedor text NOT NULL,
                id_venta int NOT NULL,
                nro_linea_detalle_venta int NOT NULL,

                CONSTRAINT pk_entradas PRIMARY KEY (id_entrada),
                CONSTRAINT fk_entradas_poseedor
                    FOREIGN KEY (id_poseedor)
                    REFERENCES usuarios_generales(usuario_id)
                    ON DELETE RESTRICT,
                CONSTRAINT fk_entradas_detalle_venta
                    FOREIGN KEY (id_venta, nro_linea_detalle_venta)
                    REFERENCES detalle_venta(id_venta, nro_linea)
                    ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS dispositivos_escaneo (
                id_dispositivo varchar(64) NOT NULL,
                id_funcionario text NOT NULL,

                CONSTRAINT pk_dispositivos_escaneo PRIMARY KEY (id_dispositivo),
                CONSTRAINT fk_dispositivos_escaneo_funcionario
                    FOREIGN KEY (id_funcionario)
                    REFERENCES funcionarios(usuario_id)
                    ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS funcionario_sector_evento (
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

            CREATE TABLE IF NOT EXISTS transferencias (
                id_transferencia serial NOT NULL,
                id_entrada uuid NOT NULL,
                id_solicitante text NOT NULL,
                id_receptor text NOT NULL,
                estado varchar(16) NOT NULL DEFAULT 'pendiente',
                fecha_solicitud timestamp NOT NULL DEFAULT NOW(),

                CONSTRAINT pk_transferencias PRIMARY KEY (id_transferencia),
                CONSTRAINT fk_transferencias_entrada
                    FOREIGN KEY (id_entrada)
                    REFERENCES entradas(id_entrada)
                    ON DELETE RESTRICT,
                CONSTRAINT fk_transferencias_solicitante
                    FOREIGN KEY (id_solicitante)
                    REFERENCES usuarios_generales(usuario_id)
                    ON DELETE RESTRICT,
                CONSTRAINT fk_transferencias_receptor
                    FOREIGN KEY (id_receptor)
                    REFERENCES usuarios_generales(usuario_id)
                    ON DELETE RESTRICT,
                CONSTRAINT ck_transferencias_estado
                    CHECK (estado IN ('pendiente', 'aceptada', 'rechazada')),
                CONSTRAINT ck_transferencias_distintos_usuarios
                    CHECK (id_solicitante <> id_receptor)
            );

            CREATE TABLE IF NOT EXISTS historial_custodia_entrada (
                id_movimiento serial NOT NULL,
                id_entrada uuid NOT NULL,
                tipo_movimiento varchar(16) NOT NULL,
                fecha_movimiento timestamp NOT NULL DEFAULT NOW(),

                CONSTRAINT pk_historial_custodia_entrada PRIMARY KEY (id_movimiento),
                CONSTRAINT fk_hce_entrada
                    FOREIGN KEY (id_entrada)
                    REFERENCES entradas(id_entrada)
                    ON DELETE RESTRICT,
                CONSTRAINT ck_hce_tipo_movimiento
                    CHECK (tipo_movimiento IN ('emision', 'transferencia'))
            );

            CREATE TABLE IF NOT EXISTS tokens_qr (
                id_token_qr integer GENERATED BY DEFAULT AS IDENTITY,
                codigo_token uuid NOT NULL,
                id_entrada uuid NOT NULL,
                fecha_expiracion timestamp NOT NULL,
                id_dispositivo varchar(64) NULL,

                CONSTRAINT pk_tokens_qr PRIMARY KEY (id_token_qr),
                CONSTRAINT fk_tokens_qr_entrada
                    FOREIGN KEY (id_entrada)
                    REFERENCES entradas(id_entrada)
                    ON DELETE CASCADE,
                CONSTRAINT fk_tokens_qr_dispositivo
                    FOREIGN KEY (id_dispositivo)
                    REFERENCES dispositivos_escaneo(id_dispositivo)
                    ON DELETE SET NULL
            );
            """);

            migrationBuilder.Sql("""
            -- Datos base del dominio del obligatorio BDII.
            -- No incluye datos internos de Identity.


            INSERT INTO paises_sede (nombre)
            VALUES
                ('Canadá'),
                ('México'),
                ('Estados Unidos')
            ON CONFLICT (nombre) DO NOTHING;

            INSERT INTO tasa_comision (tasa, fecha_desde)
            SELECT 0.0500, TIMESTAMP '2026-01-01 00:00:00'
            WHERE NOT EXISTS (
                SELECT 1
                FROM tasa_comision
                WHERE tasa = 0.0500
                  AND fecha_desde = TIMESTAMP '2026-01-01 00:00:00'
            );

            INSERT INTO equipos (nombre)
            VALUES
                ('Alemania'),
                ('Arabia Saudita'),
                ('Argelia'),
                ('Argentina'),
                ('Australia'),
                ('Austria'),
                ('Bélgica'),
                ('Bosnia y Herzegovina'),
                ('Brasil'),
                ('Cabo Verde'),
                ('Canadá'),
                ('Catar'),
                ('Chequia'),
                ('Colombia'),
                ('Corea del Sur'),
                ('Costa de Marfil'),
                ('Croacia'),
                ('Curazao'),
                ('Ecuador'),
                ('Egipto'),
                ('Escocia'),
                ('España'),
                ('Estados Unidos'),
                ('Francia'),
                ('Ghana'),
                ('Haití'),
                ('Inglaterra'),
                ('Irak'),
                ('Irán'),
                ('Japón'),
                ('Jordania'),
                ('Marruecos'),
                ('México'),
                ('Noruega'),
                ('Nueva Zelanda'),
                ('Países Bajos'),
                ('Panamá'),
                ('Paraguay'),
                ('Portugal'),
                ('República Democrática del Congo'),
                ('Senegal'),
                ('Sudáfrica'),
                ('Suecia'),
                ('Suiza'),
                ('Túnez'),
                ('Turquía'),
                ('Uruguay'),
                ('Uzbekistán')
            ON CONFLICT (nombre) DO NOTHING;
            """);

            migrationBuilder.Sql("""
            -- Indices propios del dominio del obligatorio BDII.
            -- Los indices de Identity son responsabilidad del esquema generado por .NET.


            CREATE INDEX IF NOT EXISTS ix_eventos_estadio_fecha
                ON eventos(id_estadio, fecha_hora);

            CREATE INDEX IF NOT EXISTS ix_entradas_poseedor
                ON entradas(id_poseedor);

            CREATE INDEX IF NOT EXISTS ix_entradas_detalle_venta
                ON entradas(id_venta, nro_linea_detalle_venta);

            CREATE INDEX IF NOT EXISTS ix_transferencias_solicitante
                ON transferencias(id_solicitante, fecha_solicitud DESC);

            CREATE INDEX IF NOT EXISTS ix_transferencias_receptor
                ON transferencias(id_receptor, fecha_solicitud DESC);

            CREATE INDEX IF NOT EXISTS ix_transferencias_entrada
                ON transferencias(id_entrada);

            CREATE UNIQUE INDEX IF NOT EXISTS ux_transferencias_entrada_pendiente
                ON transferencias(id_entrada)
                WHERE estado = 'pendiente';

            CREATE INDEX IF NOT EXISTS ix_hce_entrada_fecha
                ON historial_custodia_entrada(id_entrada, fecha_movimiento, id_movimiento);

            CREATE INDEX IF NOT EXISTS ix_hce_entrada_tipo
                ON historial_custodia_entrada(id_entrada, tipo_movimiento);

            CREATE INDEX IF NOT EXISTS ix_tokens_qr_entrada
                ON tokens_qr(id_entrada);

            CREATE INDEX IF NOT EXISTS ix_tokens_qr_dispositivo
                ON tokens_qr(id_dispositivo);

            CREATE UNIQUE INDEX IF NOT EXISTS ux_tokens_qr_entrada_validada
                ON tokens_qr(id_entrada)
                WHERE id_dispositivo IS NOT NULL;

            CREATE UNIQUE INDEX IF NOT EXISTS ux_tokens_qr_entrada_activa
                ON tokens_qr(id_entrada)
                WHERE id_dispositivo IS NULL;

            CREATE UNIQUE INDEX IF NOT EXISTS ux_tokens_qr_codigo_token_activo
                ON tokens_qr(codigo_token)
                WHERE id_dispositivo IS NULL;
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
            DROP TABLE IF EXISTS tokens_qr;
            DROP TABLE IF EXISTS historial_custodia_entrada;
            DROP TABLE IF EXISTS transferencias;
            DROP TABLE IF EXISTS funcionario_sector_evento;
            DROP TABLE IF EXISTS dispositivos_escaneo;
            DROP TABLE IF EXISTS entradas;
            DROP TABLE IF EXISTS detalle_venta;
            DROP TABLE IF EXISTS ventas;
            DROP TABLE IF EXISTS tasa_comision;
            DROP TABLE IF EXISTS evento_habilita_sector;
            DROP TABLE IF EXISTS equipo_juega_evento;
            DROP TABLE IF EXISTS eventos;
            DROP TABLE IF EXISTS equipos;
            DROP TABLE IF EXISTS administrador_asignado_a_pais_sede;
            DROP TABLE IF EXISTS sectores;
            DROP TABLE IF EXISTS estadios;
            DROP TABLE IF EXISTS paises_sede;
            DROP TABLE IF EXISTS telefonos_usuario;
            DROP TABLE IF EXISTS administradores;
            DROP TABLE IF EXISTS funcionarios;
            DROP TABLE IF EXISTS usuarios_generales;
            DROP TABLE IF EXISTS usuarios;
            DROP SEQUENCE IF EXISTS funcionarios_legajo_seq;
            """);

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
