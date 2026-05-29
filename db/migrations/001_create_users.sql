CREATE TABLE usuarios (
    email varchar(64) NOT NULL,
    nro_documento varchar(64) NOT NULL,
    tipo_documento varchar(16) NOT NULL,
    pais_documento varchar(32) NOT NULL,
    pais_direccion varchar(32) NOT NULL,
    localidad varchar(32) NOT NULL,
    calle varchar(32) NOT NULL,
    nro_direccion varchar(4) NOT NULL,
    codigo_postal varchar(5) NOT NULL,

    CONSTRAINT pk_usuarios PRIMARY KEY (email),
    CONSTRAINT uq_usuarios_documento
        UNIQUE (nro_documento, tipo_documento, pais_documento)
);

CREATE TABLE usuarios_generales (
    email_usuario VARCHAR(64) NOT NULL,
    fecha_registro DATE NOT NULL DEFAULT CURRENT_DATE,
    estado_identidad VARCHAR(16) NOT NULL,

    CONSTRAINT pk_usuarios_generales PRIMARY KEY (email_usuario),
    CONSTRAINT fk_usuarios_generales_usuarios
        FOREIGN KEY (email_usuario) REFERENCES usuarios(email),
    CONSTRAINT ck_usuarios_generales_estado_identidad
        CHECK (estado_identidad IN ('PENDIENTE', 'VERIFICADA', 'RECHAZADA'))
);

CREATE TABLE funcionarios (
    email_funcionario varchar(64) NOT NULL,
    nro_legajo varchar(16) NOT NULL,

    CONSTRAINT pk_funcionarios PRIMARY KEY (email_funcionario),
    CONSTRAINT uq_funcionarios_nro_legajo UNIQUE (nro_legajo),
    CONSTRAINT fk_funcionarios_usuarios
        FOREIGN KEY (email_funcionario) REFERENCES usuarios(email)
);

CREATE TABLE administradores (
    email_administrador varchar(64) NOT NULL,
    fecha_asignacion DATE NOT NULL,

    CONSTRAINT pk_administradores PRIMARY KEY (email_administrador),
    CONSTRAINT fk_administradores_usuarios
        FOREIGN KEY (email_administrador) REFERENCES usuarios(email)
);

CREATE TABLE telefonos_usuario (
    email_usuario varchar(64) NOT NULL,
    telefono varchar(20) NOT NULL,

    CONSTRAINT pk_telefonos_usuario PRIMARY KEY (email_usuario, telefono),
    CONSTRAINT fk_telefonos_usuario_usuarios
        FOREIGN KEY (email_usuario) REFERENCES usuarios(email)
);



