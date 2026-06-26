# Guia de tests de servicios

Esta carpeta contiene las pruebas automatizadas de la capa de servicios de
`TicketingMundialUCU`.

El objetivo principal de estas pruebas es verificar las reglas de negocio antes
de involucrar a la base de datos real. Por ejemplo:

- rechazar datos invalidos;
- transformar o filtrar datos antes de guardarlos;
- devolver el resultado correcto;
- llamar al DAO con los argumentos correctos;
- no llamar al DAO cuando una validacion falla;
- traducir errores tecnicos a mensajes comprensibles.

Actualmente hay **81 casos de prueba**. Un metodo marcado con `[Theory]` cuenta
como varios casos porque se ejecuta una vez por cada `[InlineData]`.

## Herramientas utilizadas

El proyecto de tests usa:

| Herramienta | Funcion |
| --- | --- |
| xUnit | Descubre y ejecuta los tests. Proporciona `[Fact]`, `[Theory]` y `Assert`. |
| NSubstitute | Crea implementaciones simuladas de los DAOs y verifica sus llamadas. |
| EF Core InMemory | Proporciona almacenamiento temporal para probar ASP.NET Core Identity sin PostgreSQL. |
| Microsoft.NET.Test.Sdk | Integra los tests con `dotnet test` y los IDE. |
| coverlet.collector | Permite recopilar cobertura de codigo. |

Las dependencias se encuentran en `../TicketingMundialUCU.Tests.csproj`.

## Resumen de la suite

| Archivo | Tipo principal | Dependencias durante el test | Casos |
| --- | --- | --- | ---: |
| `VentaServiceTests.cs` | Unitario | `IVentaDao` e `IEntradaDao` simulados | 10 |
| `EventoServiceTests.cs` | Unitario | `IEventoDao`, `IEstadioDao` y contexto de administrador simulados | 15 |
| `EstadioServiceTests.cs` | Unitario | `IEstadioDao` y contexto de administrador simulados | 6 |
| `UserPhoneServiceTests.cs` | Unitario | `IUserPhoneDao` simulado | 4 |
| `UserRolesTests.cs` | Unitario puro | Sin dependencias | 6 |
| `UserRegistrationServiceTests.cs` | Integracion parcial | Identity y EF InMemory reales; DAOs propios simulados | 11 |
| `QrCodeServiceTests.cs` | Unitario | Sin dependencias externas | 2 |
| `TokenQrServiceTests.cs` | Unitario | `ITokenQrDao` simulado | 3 |
| `TokenQrRefreshWorkerTests.cs` | Unitario | DI real acotada y `ITokenQrDao` simulado | 1 |
| `TransferenciaServiceTests.cs` | Unitario | `ITransferenciaDao` simulado | 10 |
| `FuncionarioServiceTests.cs` | Unitario | `IFuncionarioDao` e `ICurrentUserContext` simulados | 13 |
| **Total** | | | **81** |

## Tipos de test utilizados

### Tests unitarios

La mayoria son tests unitarios. Prueban una clase de servicio aislada y
reemplazan sus DAOs por substitutes de NSubstitute.

Ejemplo conceptual:

```text
Test -> servicio real -> DAO simulado
```

Pertenecen a este grupo:

- `VentaServiceTests.cs`
- `EventoServiceTests.cs`
- `EstadioServiceTests.cs`
- `UserPhoneServiceTests.cs`
- `UserRolesTests.cs`
- `QrCodeServiceTests.cs`
- `TokenQrServiceTests.cs`
- `TokenQrRefreshWorkerTests.cs`
- `TransferenciaServiceTests.cs`
- `FuncionarioServiceTests.cs`

Estos tests son rapidos y deterministas porque no necesitan red, PostgreSQL ni
datos compartidos.

### Test de integracion parcial

`UserRegistrationServiceTests.cs` combina componentes reales y simulados:

```text
Test -> UserRegistrationService real
     -> UserManager, RoleManager e Identity reales
     -> EF Core InMemory
     -> DAOs propios simulados
```

Es una integracion parcial porque prueba la colaboracion real con ASP.NET Core
Identity, pero no usa PostgreSQL ni las implementaciones reales de
`IUserDao` e `IUserPhoneDao`.

## Patron de testing

Los tests siguen el patron **Arrange, Act, Assert (AAA)**.

### Arrange: preparar

Se crean los datos de entrada y se configura el comportamiento de las
dependencias.

```csharp
var tasa = new TasaComision(4, 0.05m, new DateTime(2026, 1, 1));
_repository.GetTasaVigenteAsync().Returns(tasa);
_repository.CreateVentaAsync(
        "usuario-1",
        Arg.Any<IEnumerable<ItemCarrito>>(),
        tasa)
    .Returns(37);
```

### Act: ejecutar

Se invoca un unico comportamiento publico de la clase bajo prueba.

```csharp
var idVenta = await _service.ComprarEntradasAsync("usuario-1", items);
```

### Assert: comprobar

Se verifica el resultado observable y, cuando es relevante, la colaboracion con
el DAO.

```csharp
Assert.Equal(37, idVenta);
await _repository.Received(1).CreateVentaAsync(
    "usuario-1",
    Arg.Any<IEnumerable<ItemCarrito>>(),
    tasa);
```

Aunque el codigo no escribe comentarios `Arrange`, `Act` y `Assert`, los bloques
se separan con lineas en blanco para que las tres etapas sean visibles.

## Estado e interacciones

Las pruebas usan dos clases de comprobaciones.

### Verificacion de estado o resultado

Comprueba lo que devuelve el metodo o la excepcion que produce.

```csharp
Assert.Equal(37, idVenta);
Assert.True(result.Succeeded);
Assert.False(UserRoles.IsValid("admin"));
Assert.Equal("Mensaje esperado", exception.Message);
```

### Verificacion de interacciones

Comprueba como colaboro el servicio con una dependencia:

```csharp
await _repository.Received(1).CreateVentaAsync(...);
await _repository.DidNotReceive().UpdateEstadoVentaAsync(...);
```

Esta segunda clase de asercion permite demostrar que:

- una operacion valida intenta persistir la informacion correcta;
- una operacion invalida termina antes de acceder a persistencia;
- una dependencia se llama exactamente una vez;
- los datos fueron normalizados o filtrados antes de enviarse al DAO.

No demuestra que el DAO real o su SQL funcionen. Para eso se necesitan
tests de integracion contra PostgreSQL.

## Conceptos de xUnit usados

### `[Fact]`

Representa un caso sin parametros.

```csharp
[Fact]
public async Task EliminarEstadio_delega_la_eliminacion_al_dao()
{
    await _service.EliminarEstadioAsync(8);

    await _repository.Received(1).DeleteEstadioAsync(8);
}
```

### `[Theory]` e `[InlineData]`

Representan una misma regla probada con distintos valores.

```csharp
[Theory]
[InlineData(0)]
[InlineData(-1)]
public async Task RegistrarEstadio_con_capacidad_no_positiva_rechaza_la_operacion(
    int capacidad)
{
    // ...
}
```

xUnit ejecuta este metodo dos veces: una con `0` y otra con `-1`. Conviene usar
`Theory` cuando cambia el dato, pero el comportamiento esperado es el mismo.

### Tests asincronos

Los servicios devuelven `Task`, por lo que los tests tambien devuelven
`async Task`. Nunca se usa `.Result` ni `.Wait()`.

```csharp
public async Task Operacion_valida_devuelve_resultado()
{
    var result = await _service.OperacionAsync();
    Assert.NotNull(result);
}
```

### Comprobacion de excepciones

`Assert.ThrowsAsync<T>` comprueba que la tarea produzca la excepcion esperada y
permite inspeccionarla.

```csharp
var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
    _service.OperacionAsync());

Assert.Equal("Mensaje esperado", exception.Message);
```

## Conceptos de NSubstitute usados

### Crear un substitute

```csharp
private readonly IEventoDao _dao =
    Substitute.For<IEventoDao>();
```

NSubstitute crea en tiempo de ejecucion un objeto que implementa la interfaz.
No ejecuta el DAO real.

### Configurar una respuesta

```csharp
_dao.ExisteSuperposicionAsync(1, fecha, null).Returns(true);
```

Cuando el servicio real haga esa llamada, el substitute devolvera `true`.
En ese momento el substitute esta actuando como un **stub**.

### Simular una excepcion

```csharp
_dao.CreateEquipoAsync("Uruguay")
    .Returns(Task.FromException(new Exception("unique constraint")));
```

Esto permite probar como reacciona el servicio ante un error de una dependencia.

### Verificar una llamada

```csharp
await _dao.Received(1).DeleteEstadioAsync(8, "México");
```

Comprueba que el metodo recibio exactamente una llamada con ese argumento. En
esta comprobacion el substitute esta actuando como un **mock**.

### Verificar que no hubo una llamada

```csharp
await _dao.DidNotReceive().CreateEventoAsync(
    Arg.Any<DateTime>(),
    Arg.Any<string>(),
    Arg.Any<int>(),
    Arg.Any<int>(),
    Arg.Any<int>(),
    Arg.Any<IEnumerable<(string Sector, decimal Precio)>>());
```

Esto comprueba la ausencia de efectos secundarios cuando falla una validacion.

### Comparar argumentos complejos

`Arg.Any<T>()` acepta cualquier valor del tipo indicado. `Arg.Is<T>()` exige que
el valor cumpla una condicion.

```csharp
Arg.Is<IEnumerable<string?>>(telefonos =>
    telefonos.SequenceEqual(new[] { "+598 91 234 567", "2900 0000" }))
```

Se usa `Arg.Is` cuando el contenido enviado al DAO forma parte del
comportamiento que se quiere garantizar.

## Convencion para nombrar tests

Los nombres siguen aproximadamente esta estructura:

```text
MetodoBajoPrueba_condicion_resultadoEsperado
```

Ejemplos:

```text
ComprarEntradas_sin_cantidades_positivas_rechaza_la_compra
ProgramarEvento_superpuesto_rechaza_la_operacion
RegistrarUsuario_valido_crea_perfil_y_asigna_el_rol
```

El nombre debe explicar el escenario sin obligar a leer la implementacion.

## Que prueba cada archivo

### `VentaServiceTests.cs`

Tipo: **unitario**, con `IVentaDao` e `IEntradaDao` simulados.

La clase crea un substitute compartido y construye un `VentaService` real:

```csharp
private readonly IVentaDao _dao = Substitute.For<IVentaDao>();
private readonly IEntradaDao _entradaDao = Substitute.For<IEntradaDao>();

public VentaServiceTests()
{
    _service = new VentaService(_dao, _entradaDao);
}
```

xUnit crea una instancia nueva de la clase por cada caso. Por eso el substitute
y su historial de llamadas no se comparten entre tests.

Casos cubiertos:

1. `ComprarEntradas_sin_cantidades_positivas_rechaza_la_compra`
   - Entrega items con cantidades `0` y `-1`.
   - Espera `InvalidOperationException`.
   - Comprueba el mensaje exacto.
   - Comprueba que ni siquiera se solicite la tasa vigente.

2. `ComprarEntradas_con_mas_de_cinco_boletos_rechaza_la_compra`
   - Entrega dos items cuya suma es seis.
   - Comprueba la regla de maximo cinco entradas por transaccion.

3. `ComprarEntradas_valida_filtra_cantidades_y_usa_la_tasa_vigente`
   - Configura una tasa y un identificador de venta devuelto por el DAO.
   - Incluye un item con cantidad cero.
   - Comprueba que el servicio devuelva el identificador `37`.
   - Comprueba que sólo los items positivos lleguen a `CreateVentaAsync`.
   - Comprueba que se use la tasa obtenida anteriormente.

4. `ActualizarEstadoVenta_con_estado_valido_actualiza_la_venta`
   - Se ejecuta para `"confirmada"` y `"paga"`.
   - Comprueba que ambos estados sean enviados al DAO.

5. `ActualizarEstadoVenta_con_estado_invalido_rechaza_la_operacion`
   - Se ejecuta para `"pendiente"`, `"cancelada"` y `""`.
   - Comprueba la excepcion y su mensaje.
   - Comprueba que no se actualice ninguna venta.

6. `GetEntradasByUsuario_delega_en_entrada_dao`
   - Comprueba que la consulta de entradas de usuario delegue en `IEntradaDao`.

7. `GetDetallesByVenta_delega_en_entrada_dao`
   - Comprueba que el detalle de una venta delegue en `IEntradaDao`.

Total: **10 casos ejecutados**.

### `EventoServiceTests.cs`

Tipo: **unitario**, con `IEventoDao`, `IEstadioDao` y contexto de administrador simulados.

Casos cubiertos:

1. `ProgramarEvento_con_seleccion_invalida_rechaza_la_operacion`
   - Ejecuta cuatro combinaciones invalidas.
   - Cubre estadio sin seleccionar, equipo local sin seleccionar, visitante sin
     seleccionar y equipos iguales.
   - Comprueba el mensaje correspondiente.
   - Comprueba que una seleccion invalida no llegue a consultar superposiciones.

2. `ProgramarEvento_sin_sectores_rechaza_la_operacion`
   - Envia una coleccion vacia.
   - Comprueba que sea obligatorio habilitar al menos un sector.

3. `ProgramarEvento_con_precio_no_positivo_rechaza_la_operacion`
   - Se ejecuta con precio cero y negativo.
   - Comprueba que todos los precios deban ser mayores que cero.

4. `ProgramarEvento_superpuesto_rechaza_la_operacion`
   - Configura el DAO para informar una superposicion.
   - Comprueba la excepcion.
   - Comprueba que no se intente crear el evento.

5. `ProgramarEvento_valido_devuelve_el_identificador_creado`
   - Configura que no existe superposicion.
   - Configura que el DAO devuelva el identificador `25`.
   - Comprueba el identificador retornado y todos los argumentos de creacion.

6. `ActualizarEvento_excluye_el_evento_actual_al_buscar_superposicion`
   - Actualiza el evento `9`.
   - Comprueba que ese mismo identificador sea enviado como exclusion al buscar
     superposiciones.
   - Comprueba que luego se actualicen los datos correctos.

7. `AgregarEquipo_duplicado_devuelve_un_mensaje_claro`
   - Simula un error de restriccion unica.
   - Comprueba que el detalle tecnico se traduzca a un mensaje de negocio.

Tambien cubre fecha/hora anterior al momento actual, hora de hoy ya pasada,
actualizacion de evento vencida y estadio fuera de la jurisdiccion del
administrador.

Total: **15 casos ejecutados**.

### `EstadioServiceTests.cs`

Tipo: **unitario**, con `IEstadioDao` y contexto de administrador simulados.

Casos cubiertos:

1. `RegistrarEstadio_con_capacidad_no_positiva_rechaza_la_operacion`
   - Se ejecuta con capacidad `0` y `-1`.
   - Comprueba que cada sector deba tener capacidad positiva.

2. `RegistrarEstadio_usa_el_pais_del_administrador`
   - Envia cuatro sectores validos.
   - Comprueba que el estadio se cree en el pais sede del administrador.

3. `ActualizarEstadio_fuera_de_jurisdiccion_rechaza_la_operacion`
   - Simula que el DAO no actualiza el estadio por pais sede.
   - Comprueba que se informe una operacion no autorizada.

4. `ActualizarEstadio_con_capacidad_no_positiva_rechaza_la_operacion`
   - Agrega un sector con capacidad cero.
   - Comprueba la excepcion y que no se actualice el estadio.

5. `EliminarEstadio_fuera_de_jurisdiccion_rechaza_la_operacion`
   - Simula que el DAO no elimina el estadio por pais sede.
   - Comprueba que se informe una operacion no autorizada.

Total: **6 casos ejecutados**.

### `UserPhoneServiceTests.cs`

Tipo: **unitario**, con `IUserPhoneDao` simulado.

Casos cubiertos:

1. `GetPhoneNumbers_delegates_to_repository`
   - Configura dos telefonos como respuesta.
   - Comprueba que el servicio devuelva la misma coleccion.
   - Comprueba que consulte al usuario correcto.

2. `UpdatePhoneNumbers_normalizes_and_removes_duplicates`
   - Envia espacios exteriores, valores vacios, `null` y duplicados.
   - Comprueba que se eliminen vacios y nulos.
   - Comprueba que se aplique `Trim`.
   - Comprueba que se eliminen duplicados conservando los valores esperados.

3. `UpdatePhoneNumbers_with_too_long_number_rejects_update`
   - Envia un telefono de 21 caracteres.
   - Comprueba el limite de 20 caracteres.
   - Comprueba que el DAO no reemplace la lista.

4. `UpdatePhoneNumbers_with_invalid_number_rejects_update`
   - Envia un texto que no cumple la validacion de telefono.
   - Comprueba la excepcion y la ausencia de persistencia.

Total: **4 casos ejecutados**.

### `UserRolesTests.cs`

Tipo: **unitario puro**. No usa mocks porque `UserRoles.IsValid` es una funcion
sin dependencias externas.

Casos cubiertos:

1. `IsValid_reconoce_los_roles_soportados`
   - Se ejecuta para `general`, `funcionario` y `administrador`.
   - Espera `true`.

2. `IsValid_rechaza_roles_no_soportados`
   - Se ejecuta para `"General"`, `"admin"` y `""`.
   - Espera `false`.
   - Tambien demuestra que la comparacion distingue mayusculas y minusculas.

Total: **6 casos ejecutados**.

### `UserRegistrationServiceTests.cs`

Tipo: **integracion parcial de servicio**, con Identity real, EF Core InMemory y
DAOs propios simulados.

Cada test llama a `CrearContexto`, que:

1. crea una coleccion nueva de servicios;
2. registra logging;
3. registra un `ApplicationDbContext` InMemory con nombre unico;
4. registra Identity, roles y stores de Entity Framework;
5. construye un `ServiceProvider` y un scope;
6. obtiene `UserManager`, `RoleManager` e `IUserStore` reales;
7. crea substitutes para los DAOs propios;
8. construye el servicio real.

El nombre de base contiene un `Guid`, lo que impide que los datos de un test se
filtren a otro. `await using` ejecuta `DisposeAsync` y libera scope y provider al
final de cada caso, incluso si una asercion falla.

Casos cubiertos:

1. `RegistrarUsuario_con_rol_invalido_no_crea_el_usuario`
   - Envia el rol `"superadmin"`.
   - Comprueba un `IdentityResult` fallido con codigo `InvalidRole`.
   - Comprueba que Identity no contenga al usuario.
   - Comprueba que no se cree el perfil propio.

2. `RegistrarUsuario_con_password_invalida_no_crea_el_perfil`
   - Envia la contraseña `"corta"`.
   - Deja que los validadores reales de Identity la rechacen.
   - Comprueba que no se cree el perfil propio.

3. `RegistrarUsuario_valido_crea_perfil_y_asigna_el_rol`
   - Se ejecuta para los tres roles soportados.
   - Comprueba que el resultado sea exitoso.
   - Busca al usuario mediante el `UserManager` real.
   - Comprueba que tenga el rol esperado.
   - Comprueba todos los datos enviados a `IUserDao`.
   - Comprueba que se agregue el telefono.

4. `RegistrarUsuario_sin_telefono_no_agrega_un_telefono_vacio`
   - Envia un telefono compuesto por espacios.
   - Comprueba que el registro sea exitoso.
   - Comprueba que no se llame al DAO de telefonos.

5. `RegistrarAdministrador_sin_pais_sede_no_crea_el_usuario`
   - Envia un administrador sin pais sede asignado.
   - Comprueba un `IdentityResult` fallido con codigo `InvalidAdministratorCountry`.
   - Comprueba que Identity no contenga al usuario.

6. `RegistrarAdministrador_con_pais_no_catalogado_no_crea_el_usuario`
   - Envia un pais sede inexistente.
   - Comprueba un `IdentityResult` fallido con codigo `InvalidAdministratorCountry`.
   - Comprueba que Identity no contenga al usuario.

7. `RegistrarUsuario_con_dato_duplicado_elimina_identity_y_explica_el_error`
   - Se ejecuta para restricciones de documento, email y una restriccion
     desconocida.
   - Simula un `PostgresException` con codigo de violacion unica.
   - Comprueba el mensaje de negocio correspondiente.
   - Comprueba el rollback compensatorio: el usuario creado en Identity es
     eliminado cuando falla la creacion del perfil.

Total: **11 casos ejecutados**.

### `QrCodeServiceTests.cs`

Tipo: **unitario**, sin dependencias externas.

Casos cubiertos:

1. `GenerateTokenQrDataUri_con_token_vacio_rechaza_la_operacion`
   - Comprueba que no se genere un QR con `Guid.Empty`.

2. `GenerateTokenQrDataUri_con_token_valido_devuelve_svg_data_uri`
   - Comprueba que un token valido genere un `data:image/svg+xml;base64`.

Total: **2 casos ejecutados**.

### `TokenQrServiceTests.cs`

Tipo: **unitario**, con `ITokenQrDao` simulado.

Casos cubiertos:

1. `RenovarTokensActivos_delega_en_el_dao`
   - Comprueba que el servicio delegue la renovacion y devuelva la cantidad renovada.

2. `GetTokenActivo_con_entrada_vacia_rechaza_la_operacion`
   - Rechaza `Guid.Empty` y no consulta el DAO.

3. `GetTokenActivo_valido_delega_en_el_dao`
   - Comprueba que una entrada valida consulte el token activo del usuario.

Total: **3 casos ejecutados**.

### `TokenQrRefreshWorkerTests.cs`

Tipo: **unitario**, con DI real acotada y `ITokenQrDao` simulado.

Casos cubiertos:

1. `StartAsync_renueva_tokens_inmediatamente_sin_esperar_intervalo`
   - Comprueba que el worker ejecute una renovacion al iniciar, antes del primer tick.

Total: **1 caso ejecutado**.

### `TransferenciaServiceTests.cs`

Tipo: **unitario**, con `ITransferenciaDao` simulado.

Casos cubiertos:

1. Solicitud sin entrada seleccionada.
2. Solicitud sin email receptor.
3. Solicitud valida con normalizacion de email.
4. Consulta de cantidad de transferencias efectivas.
5. Aceptacion con id invalido.
6. Rechazo con id invalido.
7. Aceptacion valida.
8. Rechazo valido.

Total: **10 casos ejecutados**.

### `FuncionarioServiceTests.cs`

Tipo: **unitario**, con `IFuncionarioDao` e `ICurrentUserContext` simulados.

Casos cubiertos:

1. Registro de dispositivo sin id.
2. Registro valido con recorte del id.
3. Eliminacion valida e inexistente de dispositivo.
4. Validacion con dispositivo no autorizado.
5. Validacion con token inexistente o expirado.
6. Validacion de entrada ya consumida.
7. Validacion sin asignacion al sector.
8. Validacion con venta no paga.
9. Validacion exitosa delegada al DAO.

Total: **13 casos ejecutados**.

## Como crear tests para una funcionalidad nueva

### 1. Identificar la unidad bajo prueba

Elegir un servicio o funcion concreta. Sus dependencias deben inyectarse por
constructor para poder sustituirlas.

```csharp
public sealed class PagoService(IPagoRepository repository)
{
    // ...
}
```

### 2. Enumerar comportamientos, no lineas de codigo

Antes de escribir tests, listar:

- camino valido;
- datos vacios o nulos;
- valores limite;
- valores negativos;
- conflictos o duplicados;
- errores de dependencias;
- efectos secundarios que no deben ocurrir;
- transformaciones realizadas antes de persistir.

Cada test debe expresar una regla observable.

### 3. Crear la clase de tests

```csharp
using NSubstitute;
using TicketingMundialUCU.Data.Repositories;
using TicketingMundialUCU.Services;

namespace TicketingMundialUCU.Tests.Services;

public sealed class PagoServiceTests
{
    private readonly IPagoRepository _repository =
        Substitute.For<IPagoRepository>();
    private readonly PagoService _service;

    public PagoServiceTests()
    {
        _service = new PagoService(_repository);
    }
}
```

### 4. Probar el camino valido

```csharp
[Fact]
public async Task ConfirmarPago_con_datos_validos_guarda_y_devuelve_el_id()
{
    // Arrange
    _repository.CreateAsync("usuario-1", 100m).Returns(42);

    // Act
    var id = await _service.ConfirmarPagoAsync("usuario-1", 100m);

    // Assert
    Assert.Equal(42, id);
    await _repository.Received(1).CreateAsync("usuario-1", 100m);
}
```

Este test comprueba resultado e interaccion. Si los argumentos fueran una
coleccion transformada, se deberia usar `Arg.Is`.

### 5. Probar una validacion

```csharp
[Theory]
[InlineData(0)]
[InlineData(-1)]
public async Task ConfirmarPago_con_importe_no_positivo_rechaza_la_operacion(
    decimal importe)
{
    // Act
    var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
        _service.ConfirmarPagoAsync("usuario-1", importe));

    // Assert
    Assert.Equal("El importe debe ser mayor a 0.", exception.Message);
    await _repository.DidNotReceive().CreateAsync(
        Arg.Any<string>(),
        Arg.Any<decimal>());
}
```

La ausencia de persistencia es tan importante como la excepcion.

### 6. Probar errores de dependencias cuando el servicio los transforma

```csharp
[Fact]
public async Task ConfirmarPago_duplicado_devuelve_un_mensaje_claro()
{
    _repository.CreateAsync("usuario-1", 100m)
        .Returns(Task.FromException<int>(
            new Exception("unique constraint")));

    var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
        _service.ConfirmarPagoAsync("usuario-1", 100m));

    Assert.Equal("El pago ya fue registrado.", exception.Message);
}
```

No es necesario probar que NSubstitute puede lanzar excepciones. El valor del
test esta en comprobar la traduccion que realiza el servicio.

### 7. Decidir si hace falta integracion real

Usar un test unitario cuando la pregunta es:

- ¿la regla de negocio acepta o rechaza correctamente?
- ¿el servicio transforma correctamente los datos?
- ¿llama a la dependencia correcta?

Usar un test de integracion con PostgreSQL cuando la pregunta es:

- ¿la consulta SQL devuelve los datos correctos?
- ¿las claves foraneas y restricciones unicas funcionan?
- ¿la transaccion confirma o revierte todos los cambios?
- ¿el mapeo entre C# y PostgreSQL es correcto?
- ¿dos operaciones concurrentes respetan disponibilidad y stock?

Un tipo no reemplaza al otro. Cubren riesgos diferentes.

## Checklist para revisar un test nuevo

- El nombre describe metodo, condicion y resultado.
- El test verifica una sola regla de negocio.
- Se distinguen claramente Arrange, Act y Assert.
- Se usa `[Theory]` cuando varios datos representan el mismo comportamiento.
- Los metodos asincronos se esperan con `await`.
- Se comprueba el resultado o excepcion observable.
- Si hay persistencia, se comprueban los argumentos enviados.
- Si hay un error previo, se comprueba que no haya efectos secundarios.
- No se simula la propia clase bajo prueba.
- No se depende del orden de ejecucion de otros tests.
- No se usa una base de datos, reloj o red compartidos sin control.
- El test fallaria si se elimina o rompe la regla que pretende proteger.

## Que no garantizan estos tests

La suite actual no verifica:

- las implementaciones reales de los DAOs;
- las consultas y comandos SQL;
- migraciones;
- funciones, triggers o procedimientos de PostgreSQL;
- restricciones reales de claves foraneas;
- transacciones reales;
- concurrencia durante la compra de entradas;
- configuracion completa de la aplicacion;
- endpoints HTTP y renderizado de la interfaz.

EF Core InMemory tampoco se comporta igual que PostgreSQL: no reproduce su SQL,
sus restricciones, sus transacciones ni todos sus detalles de comparacion.

Por tanto, que los 81 tests pasen significa que las reglas cubiertas se
comportan correctamente bajo las dependencias configuradas. No significa que
todo el sistema y la base de datos esten verificados de extremo a extremo.

## Ejecucion

Desde `src/TicketingMundialUCU.Tests`:

```bash
dotnet test
```

Desde esta carpeta:

```bash
dotnet test ../TicketingMundialUCU.Tests.csproj
```

Ejecutar solo una clase:

```bash
dotnet test ../TicketingMundialUCU.Tests.csproj \
  --filter FullyQualifiedName~VentaServiceTests
```

Ejecutar un metodo concreto:

```bash
dotnet test ../TicketingMundialUCU.Tests.csproj \
  --filter FullyQualifiedName~ComprarEntradas_sin_cantidades_positivas
```

Recopilar cobertura:

```bash
dotnet test ../TicketingMundialUCU.Tests.csproj \
  --collect:"XPlat Code Coverage"
```

La cobertura ayuda a encontrar codigo no ejecutado, pero un porcentaje alto no
garantiza buenas aserciones. La calidad depende de que cada test proteja un
comportamiento relevante y pueda detectar una regresion real.
