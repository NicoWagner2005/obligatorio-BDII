# Diagramas del sistema

Estos diagramas complementan el MER existente en
[`docs/diagrama-MER-obligatorio.drawio`](diagrama-MER-obligatorio.drawio) y
describen la arquitectura implementada en el proyecto.

## Diagrama de componentes

```mermaid
flowchart TD
    Browser["Navegador del usuario"]

    subgraph Server["Aplicacion ASP.NET Core / Blazor Server"]
        Razor["Razor Components\nPaginas y layouts"]
        Identity["ASP.NET Core Identity\nAutenticacion y roles"]
        Services["Servicios de dominio\nReglas de negocio"]
        Worker["TokenQrRefreshWorker\nRenovacion cada 30 segundos"]
        Qr["QrCodeService\nGeneracion SVG QR"]
        Daos["DAOs Dapper\nAcceso SQL del dominio"]
        DbContext["ApplicationDbContext\nIdentity y migraciones"]
    end

    subgraph Database["PostgreSQL"]
        IdentityTables["Tablas AspNet*\nUsuarios, roles, claims"]
        DomainTables["Tablas de dominio\nventas, entradas, eventos, QR"]
    end

    QrCoder["QRCoder"]

    Browser -->|"HTTP / SignalR"| Razor
    Razor --> Identity
    Razor --> Services
    Services --> Daos
    Services --> Qr
    Worker --> Services
    Qr --> QrCoder
    Identity --> DbContext
    DbContext --> IdentityTables
    DbContext --> DomainTables
    Daos -->|"Npgsql + Dapper"| DomainTables
    Daos -->|"consultas puntuales"| IdentityTables
```

## Diagrama de clases y capas

```mermaid
classDiagram
    class IdentityUser
    class IdentityDbContext~ApplicationUser~
    class BackgroundService

    class ApplicationUser
    class ApplicationDbContext {
        #OnModelCreating(builder) void
    }
    class UserRegistrationService {
        +RegisterUserAsync(registrationData) IdentityResult
    }
    class AdministratorJurisdictionService {
        +GetHostCountriesAsync() IEnumerable~PaisSede~
        +GetCurrentCountryAsync() string
        +GetCurrentScopeAsync() AdministratorScope
        +ValidateRegistrationCountryAsync(role, countryName) IdentityResult
    }
    class EstadioService {
        +GetCurrentCountryAsync() string
        +GetAllEstadiosAsync() IEnumerable~Estadio~
        +GetSectoresByEstadioAsync(idEstadio) IEnumerable~Sector~
        +RegistrarEstadioAsync(nombre, sectores) void
        +ActualizarEstadioAsync(idEstadio, nombre, sectores) void
        +EliminarEstadioAsync(idEstadio) void
    }
    class EventoService {
        +GetCurrentCountryAsync() string
        +GetAllEquiposAsync() IEnumerable~Equipo~
        +GetAllEstadiosAsync() IEnumerable~Estadio~
        +GetSectoresByEstadioAsync(idEstadio) IEnumerable~Sector~
        +GetAllEventosDetalladosAsync() IEnumerable~EventoDetalle~
        +GetManagedEventosAsync() IEnumerable~EventoDetalle~
        +GetAllSectoresHabilitadosAsync() Dictionary
        +GetManagedSectoresHabilitadosAsync() Dictionary
        +AgregarEquipoAsync(nombre) void
        +ProgramarEventoAsync(fechaHora, idEstadio, idEquipoLocal, idEquipoVisitante, sectoresConPrecio) int
        +ActualizarEventoAsync(idEvento, fechaHora, idEstadio, idEquipoLocal, idEquipoVisitante, sectoresConPrecio) void
        +EliminarEventoAsync(idEvento) void
    }
    class VentaService {
        +GetTasaVigenteAsync() TasaComision
        +GetDisponibilidadAsync(idEvento, idEstadio) Dictionary
        +GetEntradasByUsuarioAsync(idUsuario) IEnumerable~EntradaDetalle~
        +GetAllVentasAsync() IEnumerable~VentaResumen~
        +GetVentasByUsuarioAsync(idUsuario) IEnumerable~VentaResumen~
        +GetDetallesByVentaAsync(idVenta) IEnumerable~EntradaDetalle~
        +ComprarEntradasAsync(idUsuario, items) int
        +ActualizarEstadoVentaAsync(idVenta, nuevoEstado) void
    }
    class TransferenciaService {
        +GetTransferenciasByUsuarioAsync(idUsuario) IEnumerable~TransferenciaDetalle~
        +GetPendientesRecibidasAsync(idUsuario) IEnumerable~TransferenciaDetalle~
        +GetCantidadTransferenciasEfectivasAsync(idsEntrada) Dictionary
        +SolicitarTransferenciaAsync(idSolicitante, idEntrada, emailReceptor) int
        +AceptarTransferenciaAsync(idTransferencia, idReceptor) void
        +RechazarTransferenciaAsync(idTransferencia, idReceptor) void
    }
    class FuncionarioService {
        +GetAllFuncionariosAsync() IEnumerable~FuncionarioInfo~
        +GetAllDispositivosAsync() IEnumerable~DispositivoAutorizado~
        +GetMisDispositivosAsync() IEnumerable~DispositivoAutorizado~
        +RegistrarDispositivoAsync(idDispositivo, idFuncionario) void
        +EliminarDispositivoAsync(idDispositivo) void
        +GetAllAsignacionesAsync() IEnumerable~AsignacionSector~
        +GetAsignacionesByEventoAsync(idEvento) IEnumerable~AsignacionSector~
        +GetMisAsignacionesAsync() IEnumerable~AsignacionSector~
        +AsignarSectorAsync(idFuncionario, idEvento, idEstadio, idSector) void
        +EliminarAsignacionAsync(idFuncionario, idEvento, idEstadio, idSector) void
        +ValidarEntradaAsync(codigoToken, idDispositivo) void
        +GetCoberturaSectoresAsync(idFuncionario, idEvento) IEnumerable~CoberturasSector~
    }
    class TokenQrService {
        +RenovarTokensActivosAsync() int
        +GetTokenActivoByEntradaAsync(idUsuario, idEntrada) TokenQrActivo
    }
    class TokenQrRefreshWorker {
        #ExecuteAsync(stoppingToken) void
    }
    class QrCodeService {
        +GenerateTokenQrDataUri(codigoToken) string
    }
    class QRCoder
    class CurrentUserContext {
        +GetRequiredAdministratorIdAsync() string
        +GetRequiredFuncionarioIdAsync() string
    }

    class IUserDao {
        <<interface>>
        +CreateAsync(identityUserId, nroDocumento, tipoDocumento, paisDocumento, paisDireccion, localidad, calle, nroDireccion, codigoPostal, role, paisSedeAsignado) void
    }
    class IUserPhoneDao {
        <<interface>>
        +AddAsync(userId, telefono) void
        +ReplaceAsync(userId, telefono) void
        +ReplaceAllAsync(userId, telefonos) void
        +GetByUserIdAsync(userId) IEnumerable~string~
        +DeleteAsync(userId, telefono) void
    }
    class IAdministratorJurisdictionDao {
        <<interface>>
        +GetHostCountriesAsync() IEnumerable~PaisSede~
        +CountryExistsAsync(countryName) bool
        +GetCountryForAdministratorAsync(administratorId) string
    }
    class IEstadioDao {
        <<interface>>
        +GetAllEstadiosAsync(nombrePaisSede) IEnumerable~Estadio~
        +GetSectoresByEstadioAsync(idEstadio, nombrePaisSede) IEnumerable~Sector~
        +BelongsToCountryAsync(idEstadio, nombrePaisSede) bool
        +CreateEstadioAsync(nombre, nombrePaisSede, sectores) int
        +UpdateEstadioAsync(idEstadio, nombre, nombrePaisSede, sectores) bool
        +DeleteEstadioAsync(idEstadio, nombrePaisSede) bool
    }
    class IEventoDao {
        <<interface>>
        +GetAllEquiposAsync() IEnumerable~Equipo~
        +CreateEquipoAsync(nombre) void
        +GetAllEventosDetalladosAsync() IEnumerable~EventoDetalle~
        +GetEventosDetalladosByCountryAsync(nombrePaisSede) IEnumerable~EventoDetalle~
        +GetAllSectoresHabilitadosAsync() Dictionary
        +GetSectoresHabilitadosByCountryAsync(nombrePaisSede) Dictionary
        +ExisteSuperposicionAsync(idEstadio, fechaHora, idEventoExcluir) bool
        +CreateEventoAsync(fechaHora, idAdministrador, idEstadio, idEquipoLocal, idEquipoVisitante, sectoresConPrecio) int
        +UpdateEventoAsync(idEvento, nombrePaisSede, fechaHora, idEstadio, idEquipoLocal, idEquipoVisitante, sectoresConPrecio) bool
        +DeleteEventoAsync(idEvento, nombrePaisSede) bool
    }
    class IVentaDao {
        <<interface>>
        +GetTasaVigenteAsync() TasaComision
        +CreateVentaAsync(idUsuario, items, tasa) int
        +GetAllVentasAsync() IEnumerable~VentaResumen~
        +GetVentasByUsuarioAsync(idUsuario) IEnumerable~VentaResumen~
        +GetDisponibilidadAsync(idEvento, idEstadio) Dictionary
        +UpdateEstadoVentaAsync(idVenta, estado) void
    }
    class IEntradaDao {
        <<interface>>
        +GetEntradasByUsuarioAsync(idUsuario) IEnumerable~EntradaDetalle~
        +GetDetallesByVentaAsync(idVenta) IEnumerable~EntradaDetalle~
    }
    class ITransferenciaDao {
        <<interface>>
        +CreateSolicitudAsync(idEntrada, idSolicitante, emailReceptor) int
        +GetTransferenciasByUsuarioAsync(idUsuario) IEnumerable~TransferenciaDetalle~
        +GetPendientesRecibidasAsync(idUsuario) IEnumerable~TransferenciaDetalle~
        +GetCantidadTransferenciasEfectivasAsync(idsEntrada) Dictionary
        +AcceptAsync(idTransferencia, idReceptor) void
        +RejectAsync(idTransferencia, idReceptor) void
    }
    class IFuncionarioDao {
        <<interface>>
        +GetAllFuncionariosAsync() IEnumerable~FuncionarioInfo~
        +GetAllDispositivosAsync() IEnumerable~DispositivoAutorizado~
        +GetDispositivosByFuncionarioAsync(idFuncionario) IEnumerable~DispositivoAutorizado~
        +IsDispositivoDelFuncionarioAsync(idDispositivo, idFuncionario) bool
        +CreateDispositivoAsync(idDispositivo, idFuncionario) void
        +DeleteDispositivoAsync(idDispositivo) bool
        +GetAllAsignacionesAsync() IEnumerable~AsignacionSector~
        +GetAsignacionesByEventoAsync(idEvento) IEnumerable~AsignacionSector~
        +GetAsignacionesByFuncionarioAsync(idFuncionario) IEnumerable~AsignacionSector~
        +ExisteAsignacionAsync(idFuncionario, idEvento, idEstadio, idSector) bool
        +CreateAsignacionAsync(idFuncionario, idEvento, idEstadio, idSector) void
        +DeleteAsignacionAsync(idFuncionario, idEvento, idEstadio, idSector) bool
        +GetEntradaParaValidarAsync(codigoToken) EntradaValidacionInfo
        +ValidarEntradaAsync(codigoToken, idFuncionario, idDispositivo) void
        +GetCoberturaSectoresAsync(idFuncionario, idEvento) IEnumerable~CoberturasSector~
    }
    class ITokenQrDao {
        <<interface>>
        +RenovarTokensActivosAsync() int
        +GetTokenActivoByEntradaAsync(idUsuario, idEntrada) TokenQrActivo
    }
    class ICurrentUserContext {
        <<interface>>
        +GetRequiredAdministratorIdAsync() string
        +GetRequiredFuncionarioIdAsync() string
    }

    class UserDao {
        +CreateAsync(identityUserId, nroDocumento, tipoDocumento, paisDocumento, paisDireccion, localidad, calle, nroDireccion, codigoPostal, role, paisSedeAsignado) void
    }
    class UserPhoneDao {
        +AddAsync(userId, telefono) void
        +ReplaceAsync(userId, telefono) void
        +ReplaceAllAsync(userId, telefonos) void
        +GetByUserIdAsync(userId) IEnumerable~string~
        +DeleteAsync(userId, telefono) void
    }
    class AdministratorJurisdictionDao {
        +GetHostCountriesAsync() IEnumerable~PaisSede~
        +CountryExistsAsync(countryName) bool
        +GetCountryForAdministratorAsync(administratorId) string
    }
    class EstadioDao {
        +GetAllEstadiosAsync(nombrePaisSede) IEnumerable~Estadio~
        +GetSectoresByEstadioAsync(idEstadio, nombrePaisSede) IEnumerable~Sector~
        +BelongsToCountryAsync(idEstadio, nombrePaisSede) bool
        +CreateEstadioAsync(nombre, nombrePaisSede, sectores) int
        +UpdateEstadioAsync(idEstadio, nombre, nombrePaisSede, sectores) bool
        +DeleteEstadioAsync(idEstadio, nombrePaisSede) bool
    }
    class EventoDao {
        +GetAllEquiposAsync() IEnumerable~Equipo~
        +CreateEquipoAsync(nombre) void
        +GetAllEventosDetalladosAsync() IEnumerable~EventoDetalle~
        +GetEventosDetalladosByCountryAsync(nombrePaisSede) IEnumerable~EventoDetalle~
        +GetAllSectoresHabilitadosAsync() Dictionary
        +GetSectoresHabilitadosByCountryAsync(nombrePaisSede) Dictionary
        +ExisteSuperposicionAsync(idEstadio, fechaHora, idEventoExcluir) bool
        +CreateEventoAsync(fechaHora, idAdministrador, idEstadio, idEquipoLocal, idEquipoVisitante, sectoresConPrecio) int
        +UpdateEventoAsync(idEvento, nombrePaisSede, fechaHora, idEstadio, idEquipoLocal, idEquipoVisitante, sectoresConPrecio) bool
        +DeleteEventoAsync(idEvento, nombrePaisSede) bool
    }
    class VentaDao {
        +GetTasaVigenteAsync() TasaComision
        +CreateVentaAsync(idUsuario, items, tasa) int
        +GetAllVentasAsync() IEnumerable~VentaResumen~
        +GetVentasByUsuarioAsync(idUsuario) IEnumerable~VentaResumen~
        +GetDisponibilidadAsync(idEvento, idEstadio) Dictionary
        +UpdateEstadoVentaAsync(idVenta, estado) void
    }
    class EntradaDao {
        +GetEntradasByUsuarioAsync(idUsuario) IEnumerable~EntradaDetalle~
        +GetDetallesByVentaAsync(idVenta) IEnumerable~EntradaDetalle~
    }
    class TransferenciaDao {
        +CreateSolicitudAsync(idEntrada, idSolicitante, emailReceptor) int
        +GetTransferenciasByUsuarioAsync(idUsuario) IEnumerable~TransferenciaDetalle~
        +GetPendientesRecibidasAsync(idUsuario) IEnumerable~TransferenciaDetalle~
        +GetCantidadTransferenciasEfectivasAsync(idsEntrada) Dictionary
        +AcceptAsync(idTransferencia, idReceptor) void
        +RejectAsync(idTransferencia, idReceptor) void
    }
    class FuncionarioDao {
        +GetAllFuncionariosAsync() IEnumerable~FuncionarioInfo~
        +GetAllDispositivosAsync() IEnumerable~DispositivoAutorizado~
        +GetDispositivosByFuncionarioAsync(idFuncionario) IEnumerable~DispositivoAutorizado~
        +IsDispositivoDelFuncionarioAsync(idDispositivo, idFuncionario) bool
        +CreateDispositivoAsync(idDispositivo, idFuncionario) void
        +DeleteDispositivoAsync(idDispositivo) bool
        +GetAllAsignacionesAsync() IEnumerable~AsignacionSector~
        +GetAsignacionesByEventoAsync(idEvento) IEnumerable~AsignacionSector~
        +GetAsignacionesByFuncionarioAsync(idFuncionario) IEnumerable~AsignacionSector~
        +ExisteAsignacionAsync(idFuncionario, idEvento, idEstadio, idSector) bool
        +CreateAsignacionAsync(idFuncionario, idEvento, idEstadio, idSector) void
        +DeleteAsignacionAsync(idFuncionario, idEvento, idEstadio, idSector) bool
        +GetEntradaParaValidarAsync(codigoToken) EntradaValidacionInfo
        +ValidarEntradaAsync(codigoToken, idFuncionario, idDispositivo) void
        +GetCoberturaSectoresAsync(idFuncionario, idEvento) IEnumerable~CoberturasSector~
    }
    class TokenQrDao {
        +RenovarTokensActivosAsync() int
        +GetTokenActivoByEntradaAsync(idUsuario, idEntrada) TokenQrActivo
    }

    %% Herencias del framework
    IdentityUser <|-- ApplicationUser
    IdentityDbContext~ApplicationUser~ <|-- ApplicationDbContext
    BackgroundService <|-- TokenQrRefreshWorker

    %% ApplicationDbContext gestiona ApplicationUser
    ApplicationDbContext --> ApplicationUser

    %% Services -> Interfaces (dependencias)
    UserRegistrationService --> IUserDao
    UserRegistrationService --> IUserPhoneDao
    UserRegistrationService --> AdministratorJurisdictionService
    AdministratorJurisdictionService --> IAdministratorJurisdictionDao
    AdministratorJurisdictionService --> ICurrentUserContext
    EstadioService --> IEstadioDao
    EstadioService --> AdministratorJurisdictionService
    EventoService --> IEventoDao
    EventoService --> IEstadioDao
    EventoService --> AdministratorJurisdictionService
    VentaService --> IVentaDao
    VentaService --> IEntradaDao
    TransferenciaService --> ITransferenciaDao
    FuncionarioService --> IFuncionarioDao
    FuncionarioService --> ICurrentUserContext
    TokenQrService --> ITokenQrDao
    TokenQrRefreshWorker --> TokenQrService
    QrCodeService ..> QRCoder

    %% Interfaces <|.. Implementaciones
    IUserDao <|.. UserDao
    IUserPhoneDao <|.. UserPhoneDao
    IAdministratorJurisdictionDao <|.. AdministratorJurisdictionDao
    IEstadioDao <|.. EstadioDao
    IEventoDao <|.. EventoDao
    IVentaDao <|.. VentaDao
    IEntradaDao <|.. EntradaDao
    ITransferenciaDao <|.. TransferenciaDao
    IFuncionarioDao <|.. FuncionarioDao
    ITokenQrDao <|.. TokenQrDao
    ICurrentUserContext <|.. CurrentUserContext
```

## Diagrama entidad-relacion implementado

```mermaid
erDiagram
    ASPNET_USERS {
        string id PK
        string email
    }

    USUARIOS {
        string id PK, FK
        string nro_documento
        string tipo_documento
        string pais_documento
        string pais_direccion
        string localidad
        string calle
        string nro_direccion
        string codigo_postal
    }

    USUARIOS_GENERALES {
        string usuario_id PK, FK
        date fecha_registro
        string estado_identidad
    }

    FUNCIONARIOS {
        string usuario_id PK, FK
        string nro_legajo UK
    }

    ADMINISTRADORES {
        string usuario_id PK, FK
        date fecha_asignacion
    }

    TELEFONOS_USUARIO {
        string usuario_id PK, FK
        string telefono PK
    }

    PAISES_SEDE {
        string nombre PK
    }

    ADMINISTRADOR_ASIGNADO_A_PAIS_SEDE {
        string id_administrador PK, FK
        string nombre_pais_sede PK, FK
    }

    ESTADIOS {
        int id_estadio PK
        string nombre
        string nombre_pais_sede FK
    }

    SECTORES {
        int id_estadio PK, FK
        string id_sector PK
        int capacidad_max
    }

    EQUIPOS {
        int id_equipo PK
        string nombre UK
    }

    EVENTOS {
        int id_evento PK
        timestamp fecha_hora
        string id_administrador FK
        int id_estadio FK
    }

    EQUIPO_JUEGA_EVENTO {
        int id_equipo PK, FK
        int id_evento PK, FK
        string rol
    }

    EVENTO_HABILITA_SECTOR {
        int id_evento PK, FK
        int id_estadio PK, FK
        string id_sector PK, FK
        decimal precio
    }

    TASA_COMISION {
        int id_tasa PK
        decimal tasa
        timestamp fecha_desde
    }

    VENTAS {
        int id_venta PK
        string id_comprador FK
        timestamp fecha_venta
        string estado
        decimal monto_total
        decimal tasa_comision_aplicada
        int id_tasa FK
    }

    DETALLE_VENTA {
        int id_venta PK, FK
        int nro_linea PK
        int id_evento FK
        int id_estadio FK
        string id_sector FK
        int cantidad
        decimal subtotal
    }

    ENTRADAS {
        uuid id_entrada PK
        string id_poseedor FK
        int id_venta FK
        int nro_linea_detalle_venta FK
    }

    DISPOSITIVOS_ESCANEO {
        string id_dispositivo PK
        string id_funcionario FK
    }

    FUNCIONARIO_SECTOR_EVENTO {
        string id_funcionario PK, FK
        int id_evento PK, FK
        int id_estadio PK, FK
        string id_sector PK, FK
    }

    TRANSFERENCIAS {
        int id_transferencia PK
        uuid id_entrada FK
        string id_solicitante FK
        string id_receptor FK
        string estado
        timestamp fecha_solicitud
    }

    HISTORIAL_CUSTODIA_ENTRADA {
        int id_movimiento PK
        uuid id_entrada FK
        string tipo_movimiento
        timestamp fecha_movimiento
    }

    TOKENS_QR {
        int id_token_qr PK
        uuid codigo_token UK
        uuid id_entrada FK
        timestamp fecha_expiracion
        string id_dispositivo FK
    }

    ASPNET_USERS ||--|| USUARIOS : extiende
    USUARIOS ||--o| USUARIOS_GENERALES : especializa
    USUARIOS ||--o| FUNCIONARIOS : especializa
    USUARIOS ||--o| ADMINISTRADORES : especializa
    USUARIOS ||--o{ TELEFONOS_USUARIO : tiene
    ADMINISTRADORES ||--o| ADMINISTRADOR_ASIGNADO_A_PAIS_SEDE : asigna
    PAISES_SEDE ||--o{ ADMINISTRADOR_ASIGNADO_A_PAIS_SEDE : contiene
    PAISES_SEDE ||--o{ ESTADIOS : contiene
    ESTADIOS ||--o{ SECTORES : divide
    ADMINISTRADORES ||--o{ EVENTOS : gestiona
    ESTADIOS ||--o{ EVENTOS : aloja
    EQUIPOS ||--o{ EQUIPO_JUEGA_EVENTO : participa
    EVENTOS ||--o{ EQUIPO_JUEGA_EVENTO : enfrenta
    EVENTOS ||--o{ EVENTO_HABILITA_SECTOR : habilita
    SECTORES ||--o{ EVENTO_HABILITA_SECTOR : ofrece
    USUARIOS_GENERALES ||--o{ VENTAS : compra
    TASA_COMISION ||--o{ VENTAS : aplica
    VENTAS ||--o{ DETALLE_VENTA : detalla
    EVENTO_HABILITA_SECTOR ||--o{ DETALLE_VENTA : vende
    DETALLE_VENTA ||--o{ ENTRADAS : emite
    USUARIOS_GENERALES ||--o{ ENTRADAS : posee
    FUNCIONARIOS ||--o{ DISPOSITIVOS_ESCANEO : usa
    FUNCIONARIOS ||--o{ FUNCIONARIO_SECTOR_EVENTO : asignado
    EVENTO_HABILITA_SECTOR ||--o{ FUNCIONARIO_SECTOR_EVENTO : cubre
    ENTRADAS ||--o{ TRANSFERENCIAS : transfiere
    USUARIOS_GENERALES ||--o{ TRANSFERENCIAS : solicita
    USUARIOS_GENERALES ||--o{ TRANSFERENCIAS : recibe
    ENTRADAS ||--o{ HISTORIAL_CUSTODIA_ENTRADA : registra
    ENTRADAS ||--o{ TOKENS_QR : genera
    DISPOSITIVOS_ESCANEO ||--o{ TOKENS_QR : valida
```

## Secuencia de compra de entradas

```mermaid
sequenceDiagram
    actor Usuario
    participant UI as ComprarEntradas.razor
    participant VentaService
    participant VentaDao
    participant DB as PostgreSQL

    Usuario->>UI: Selecciona evento, sector y cantidad
    UI->>VentaService: ComprarEntradasAsync(idUsuario, items)
    VentaService->>VentaService: Filtra cantidades y valida maximo 5 entradas
    VentaService->>VentaDao: GetTasaVigenteAsync()
    VentaDao->>DB: SELECT tasa_comision vigente
    DB-->>VentaDao: tasa
    VentaService->>VentaDao: CreateVentaAsync(idUsuario, items, tasa)
    VentaDao->>DB: Consulta precios por sector habilitado
    VentaDao->>DB: Verifica capacidad disponible por sector
    VentaDao->>DB: INSERT ventas
    VentaDao->>DB: INSERT detalle_venta
    loop Por cada boleto comprado
        VentaDao->>DB: INSERT entradas
        VentaDao->>DB: INSERT historial_custodia_entrada(emision)
    end
    VentaDao-->>VentaService: idVenta
    VentaService-->>UI: idVenta
    UI-->>Usuario: Compra creada en estado pendiente
```

## Secuencia de validacion de entrada QR

```mermaid
sequenceDiagram
    actor Funcionario
    participant UI as ValidarEntrada.razor
    participant FuncionarioService
    participant CurrentUserContext
    participant FuncionarioDao
    participant DB as PostgreSQL

    Funcionario->>UI: Escanea QR dinamico
    UI->>FuncionarioService: ValidarEntradaAsync(codigoToken, idDispositivo)
    FuncionarioService->>CurrentUserContext: GetRequiredFuncionarioIdAsync()
    CurrentUserContext-->>FuncionarioService: idFuncionario
    FuncionarioService->>FuncionarioDao: IsDispositivoDelFuncionarioAsync()
    FuncionarioDao->>DB: Verifica dispositivo autorizado
    DB-->>FuncionarioDao: resultado
    FuncionarioService->>FuncionarioDao: GetEntradaParaValidarAsync(codigoToken)
    FuncionarioDao->>DB: Busca token activo no expirado
    DB-->>FuncionarioDao: entrada, evento, sector y estado
    FuncionarioService->>FuncionarioService: Valida no consumida, sector asignado y venta paga
    FuncionarioService->>FuncionarioDao: ValidarEntradaAsync(codigoToken, idFuncionario, idDispositivo)
    FuncionarioDao->>DB: Bloquea token/entrada/venta en transaccion
    FuncionarioDao->>DB: Marca token con id_dispositivo
    FuncionarioDao-->>FuncionarioService: Validacion confirmada
    FuncionarioService-->>UI: OK
    UI-->>Funcionario: Entrada validada
```

## Secuencia de transferencia de entrada

```mermaid
sequenceDiagram
    actor Origen as Usuario origen
    actor Receptor as Usuario receptor
    participant UI as Transferencias.razor
    participant TransferenciaService
    participant TransferenciaDao
    participant DB as PostgreSQL

    Origen->>UI: Solicita transferir entrada a un email
    UI->>TransferenciaService: SolicitarTransferenciaAsync(idSolicitante, idEntrada, email)
    TransferenciaService->>TransferenciaService: Valida entrada y email
    TransferenciaService->>TransferenciaDao: CreateSolicitudAsync()
    TransferenciaDao->>DB: Busca receptor usuario general
    TransferenciaDao->>DB: Bloquea entrada y valida poseedor actual
    TransferenciaDao->>DB: Verifica no validada, maximo 3 transferencias y sin pendiente
    TransferenciaDao->>DB: INSERT transferencias estado pendiente
    TransferenciaDao-->>UI: idTransferencia

    Receptor->>UI: Acepta transferencia
    UI->>TransferenciaService: AceptarTransferenciaAsync(idTransferencia, idReceptor)
    TransferenciaService->>TransferenciaDao: AcceptAsync()
    TransferenciaDao->>DB: Bloquea transferencia y entrada
    TransferenciaDao->>DB: UPDATE entradas.id_poseedor
    TransferenciaDao->>DB: UPDATE transferencias estado aceptada
    TransferenciaDao->>DB: INSERT historial_custodia_entrada(transferencia)
    TransferenciaDao-->>UI: OK
```

## Notas para el informe

- El MER editable esta en `docs/diagrama-MER-obligatorio.drawio`.
- El diagrama entidad-relacion anterior refleja las tablas creadas por la
  migracion `20260621201820_InitialSchema`.
- El diagrama de clases prioriza servicios, interfaces y DAOs porque esas son
  las clases propias donde se concentran las reglas del obligatorio.
- Los diagramas de secuencia documentan los casos de uso con mas reglas de
  integridad: compra, validacion QR y transferencia.
