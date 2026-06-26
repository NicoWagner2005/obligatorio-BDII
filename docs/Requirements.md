# Analisis de Requerimientos - Sistema de Ticketing Mundial 2026

## 1. Alcance del sistema

El sistema a desarrollar es una plataforma cliente/servidor de ticketing para eventos deportivos de alta concurrencia, orientada a la comercializacion, transferencia, tenencia y validacion de entradas digitales dinamicas.

La solucion debe contemplar:

- Registro y gestion de usuarios.
- Control de acceso basado en roles.
- Administracion de estadios, sectores y eventos.
- Venta de entradas.
- Transferencia de entradas entre usuarios.
- Generacion y validacion de entradas dinamicas mediante token/QR.
- Auditoria de accesos y transferencias.
- Consultas y estadisticas basicas.
- Persistencia en una base de datos SQL residente en Linux.

---

## 2. Actores

### ACT-1: Usuario general

Usuario final de la plataforma. Puede registrarse, comprar entradas, recibir entradas transferidas, transferir entradas propias, visualizar sus compras, visualizar sus transferencias y consultar las entradas que tiene asignadas.

### ACT-2: Administrador por pais sede

Usuario con permisos administrativos limitados a su jurisdiccion geografica. Puede gestionar estadios, sectores, eventos, dispositivos autorizados y asignaciones operativas dentro de su pais sede.

### ACT-3: Funcionario de validacion

Usuario operativo encargado de validar entradas en los accesos a los estadios. Debe estar vinculado a un dispositivo autorizado y puede validar entradas en los sectores que le fueron asignados para un evento.

### ACT-4: Sistema

Actor interno responsable de ejecutar procesos automaticos, como generar identificadores, recalcular tokens dinamicos, registrar auditoria, controlar restricciones de aforo, validar estados y preservar historiales.

---

## 3. Business Use Cases (BUC)

### BUC-1: Gestionar usuarios y perfiles

Comprende el registro de usuarios, la gestion de sus datos personales y la asignacion de roles dentro del sistema.

### BUC-2: Gestionar autenticacion y autorizacion

Comprende el inicio y cierre de sesion, el control de acceso basado en roles y la restriccion de operaciones segun perfil y jurisdiccion.

### BUC-3: Gestionar infraestructura deportiva

Comprende el alta y modificacion de estadios, sectores, capacidades, costos y jurisdicciones asociadas.

### BUC-4: Gestionar eventos deportivos

Comprende la programacion y modificacion de partidos, la asignacion de estadio, fecha, hora, equipos participantes y sectores habilitados.

### BUC-5: Gestionar ventas de entradas

Comprende la compra de entradas, el calculo de montos, la aplicacion de comisiones, la emision de entradas individuales y la consulta de compras realizadas.

### BUC-6: Gestionar tenencia y transferencia de entradas

Comprende la asignacion inicial de entradas, la transferencia entre usuarios, la aceptacion de transferencias y el mantenimiento de la cadena historica de custodia.

### BUC-7: Gestionar entrada dinamica

Comprende la generacion periodica de tokens dinamicos o codigos QR dinamicos para evitar fraude por capturas de pantalla o codigos estaticos.

### BUC-8: Gestionar dispositivos de validacion

Comprende el registro de dispositivos autorizados, su vinculacion obligatoria a funcionarios de validacion y la asignacion de funcionarios a sectores durante eventos.

### BUC-9: Validar accesos a eventos

Comprende el escaneo, verificacion, consumo irreversible de entradas y registro completo de auditoria del acceso.

#### Aclaracion sobre el scanner QR

El scanner QR es una funcionalidad de la interfaz del funcionario de validacion. Antes de escanear, el funcionario debe seleccionar uno de sus dispositivos autorizados. Al iniciar el scanner, el navegador solicita permiso de camara y usa la API de lectura de codigos QR disponible en el cliente. Si la camara, el permiso o la API del navegador no estan disponibles, el sistema permite ingresar manualmente el token mostrado junto al QR.

El QR no valida la entrada por si mismo: solo transporta el token dinamico activo de la entrada. Cuando el scanner detecta un QR, la interfaz extrae el token, detiene la camara y envia el token junto con el dispositivo seleccionado al servidor. La validacion definitiva verifica que el dispositivo pertenezca al funcionario, que el token exista y no haya expirado, que la venta este paga, que la entrada no haya sido consumida y que el funcionario este asignado al sector correspondiente. Si la validacion es exitosa, la entrada queda consumida de forma irreversible.

### BUC-10: Consultar reportes y estadisticas

Comprende la visualizacion de eventos con mas entradas vendidas y el ranking de mayores compradores.

---

## 4. Product Use Cases (PUC)

### BUC-1: Gestionar usuarios y perfiles

- PUC-1: Registrarse como usuario.
- PUC-2: Registrar datos personales del usuario.
- PUC-3: Gestionar perfiles y roles.
- PUC-4: Registrar datos especificos de administrador por pais sede.
- PUC-5: Registrar datos especificos de funcionario de validacion.

### BUC-2: Gestionar autenticacion y autorizacion

- PUC-6: Iniciar sesion.
- PUC-7: Cerrar sesion.
- PUC-8: Controlar permisos por rol.
- PUC-9: Controlar jurisdiccion del administrador por pais sede.

### BUC-3: Gestionar infraestructura deportiva

- PUC-10: Registrar estadio.
- PUC-11: Modificar estadio.
- PUC-12: Definir sectores de estadio.
- PUC-13: Configurar capacidad maxima por sector.
- PUC-14: Configurar costo de entrada por sector.

### BUC-4: Gestionar eventos deportivos

- PUC-15: Programar evento.
- PUC-16: Modificar evento.
- PUC-17: Asignar estadio a evento.
- PUC-18: Definir equipos local y visitante.
- PUC-19: Definir fecha y hora exacta del evento.
- PUC-20: Habilitar sectores para un evento.
- PUC-21: Impedir superposicion de eventos en un mismo estadio.

### BUC-5: Gestionar ventas de entradas

- PUC-22: Comprar entradas.
- PUC-23: Emitir entradas individuales.
- PUC-24: Calcular monto total de venta.
- PUC-25: Aplicar comision vigente.
- PUC-26: Consultar compras realizadas.

### BUC-6: Gestionar tenencia y transferencia de entradas

- PUC-27: Consultar entradas asignadas.
- PUC-28: Iniciar transferencia de entrada.
- PUC-29: Aceptar transferencia de entrada.
- PUC-30: Consultar transferencias realizadas.
- PUC-31: Consultar transferencias recibidas.
- PUC-32: Consultar historial de custodia de una entrada.

### BUC-7: Gestionar entrada dinamica

- PUC-33: Generar token dinamico de entrada.
- PUC-34: Regenerar token dinamico periodicamente.

### BUC-8: Gestionar dispositivos de validacion

- PUC-35: Registrar dispositivo autorizado.
- PUC-36: Vincular dispositivo autorizado a funcionario de validacion.
- PUC-37: Asignar funcionario a sectores de un evento.
- PUC-38: Consultar dispositivos registrados.

### BUC-9: Validar accesos a eventos

- PUC-39: Escanear entrada dinamica.
- PUC-40: Validar token activo.
- PUC-41: Registrar acceso validado.
- PUC-42: Consumir entrada de forma irreversible.
- PUC-43: Verificar cobertura de sectores asignados a un funcionario.

### BUC-10: Consultar reportes y estadisticas

- PUC-44: Consultar eventos con mas entradas vendidas.
- PUC-45: Consultar ranking de mayores compradores.

---

## 5. Requerimientos funcionales

### Usuarios, perfiles y datos personales

- RF-1: El sistema debe permitir registrar usuarios mediante un mail identificador.
- RF-2: El sistema debe impedir registrar dos usuarios con el mismo mail.
- RF-3: El sistema debe registrar el documento del usuario, compuesto por pais, tipo de documento y numero.
- RF-4: El sistema debe impedir registrar documentos duplicados.
- RF-5: El sistema debe registrar la direccion del usuario, compuesta por pais, localidad, calle, numero y codigo postal.
- RF-6: El sistema debe permitir registrar multiples telefonos de contacto por usuario.
- RF-7: El sistema debe registrar la fecha de registro de los usuarios generales.
- RF-8: El sistema debe registrar el estado de verificacion de identidad de los usuarios generales.
- RF-9: El sistema debe implementar perfiles de usuario diferenciados.
- RF-10: El sistema debe soportar los roles Administrador por Pais Sede, Funcionario de Validacion y Usuario General.
- RF-11: El sistema debe registrar la fecha de asignacion al cargo de cada administrador por pais sede.
- RF-12: El sistema debe asociar cada administrador por pais sede a una jurisdiccion geografica.
- RF-13: El sistema debe registrar el numero de legajo de cada funcionario de validacion.

### Autenticacion y autorizacion

- RF-14: El sistema debe permitir iniciar sesion.
- RF-15: El sistema debe permitir cerrar sesion.
- RF-16: El sistema debe controlar el acceso a funcionalidades segun el rol del usuario autenticado.
- RF-17: El sistema debe impedir que un usuario ejecute operaciones no autorizadas para su rol.
- RF-18: El sistema debe impedir que un administrador gestione estadios o eventos fuera de su jurisdiccion geografica.

### Estadios y sectores

- RF-19: El sistema debe permitir al administrador registrar estadios.
- RF-20: El sistema debe permitir al administrador modificar estadios.
- RF-21: El sistema debe asociar cada estadio a un pais sede o jurisdiccion geografica.
- RF-22: El sistema debe desglosar cada estadio en sectores A, B, C y D.
- RF-23: El sistema debe permitir parametrizar la capacidad maxima de cada sector.
- RF-24: El sistema debe permitir configurar el costo de entrada de cada sector.
- RF-25: El sistema debe usar la capacidad maxima del sector como limite duro para la emision de entradas.

### Eventos deportivos

- RF-26: El sistema debe permitir al administrador programar eventos deportivos.
- RF-27: El sistema debe permitir modificar eventos deportivos.
- RF-28: El sistema debe registrar el equipo local y el equipo visitante de cada evento.
- RF-29: El sistema debe vincular cada evento a un estadio especifico.
- RF-30: El sistema debe registrar la fecha y hora exacta de cada evento.
- RF-31: El sistema debe impedir la superposicion de eventos en un mismo estadio.
- RF-32: El sistema debe permitir habilitar uno o mas sectores del estadio para cada evento.
- RF-33: El sistema debe impedir vender entradas para sectores no habilitados en el evento.

### Ventas y emision de entradas

- RF-34: El sistema debe permitir que un usuario compre entradas.
- RF-35: El sistema debe permitir incluir multiples entradas en una misma venta.
- RF-36: El sistema debe permitir incluir entradas de distintos sectores en una misma venta.
- RF-37: El sistema debe impedir comprar mas de 5 entradas en una misma transaccion.
- RF-38: El sistema debe registrar la fecha de cada venta.
- RF-39: El sistema debe registrar el estado de cada venta.
- RF-40: El sistema debe manejar, como minimo, los estados de venta pendiente, confirmada y paga.
- RF-41: El sistema debe calcular el monto total de la venta en base al costo de las entradas mas una comision.
- RF-42: El sistema debe aplicar inicialmente una comision del 5% sobre el total de la venta.
- RF-43: El sistema debe permitir que la tasa de comision varie a lo largo del tiempo.
- RF-44: El sistema debe conservar la tasa de comision aplicada a cada venta.
- RF-45: El sistema debe generar una entrada individual por cada boleto comprado.
- RF-46: El sistema debe generar un identificador unico para cada entrada.
- RF-47: El sistema debe asociar cada entrada a una venta.
- RF-48: El sistema debe asociar cada entrada a un evento.
- RF-49: El sistema debe asociar cada entrada a un sector habilitado del evento.
- RF-50: El sistema debe registrar al comprador original de cada entrada.
- RF-51: El sistema debe asignar inicialmente cada entrada al usuario comprador.
- RF-52: El sistema debe impedir emitir entradas por encima del aforo disponible del sector para el evento.
- RF-53: El sistema debe permitir al usuario visualizar sus compras realizadas.

### Tenencia y transferencia de entradas

- RF-54: El sistema debe permitir visualizar las entradas actualmente asignadas a un usuario.
- RF-55: El sistema debe permitir que un usuario inicie la transferencia de una entrada propia a otro usuario.
- RF-56: El sistema debe verificar que la entrada pertenezca actualmente al usuario que intenta transferirla.
- RF-57: El sistema debe impedir transferir una entrada ya consumida o validada.
- RF-58: El sistema debe impedir transferir una entrada que ya fue transferida 3 veces.
- RF-59: El sistema debe permitir que el usuario destinatario acepte la transferencia.
- RF-60: El sistema debe cambiar el titular actual de la entrada cuando la transferencia sea aceptada.
- RF-61: El sistema debe registrar cada transferencia de entrada.
- RF-62: El sistema debe registrar el usuario origen, el usuario destino, la entrada y la fecha de cada transferencia.
- RF-63: El sistema debe permitir visualizar las transferencias realizadas por un usuario.
- RF-64: El sistema debe permitir visualizar las transferencias recibidas por un usuario.
- RF-65: El sistema debe permitir reconstruir la cadena de custodia de una entrada desde su emision original hasta su validacion final.

### Entrada dinamica y seguridad contra fraude

- RF-66: El sistema debe generar un token dinamico para cada entrada.
- RF-67: El sistema debe representar la entrada mediante un codigo QR dinamico o mecanismo equivalente basado en el token activo.
- RF-68: El sistema debe regenerar el token de la entrada cada 30 segundos mientras la aplicacion este en primer plano.
- RF-69: El sistema debe evitar que una captura de pantalla o codigo estatico sea suficiente para validar una entrada.
- RF-70: El sistema debe conservar la informacion necesaria para verificar la validez del token activo al momento del escaneo.

### Dispositivos y funcionarios de validacion

- RF-71: El sistema debe permitir registrar IDs de dispositivos de escaneo autorizados.
- RF-72: El sistema debe permitir consultar los dispositivos de escaneo registrados.
- RF-73: El sistema debe vincular obligatoriamente cada dispositivo autorizado a un funcionario de validacion.
- RF-74: El sistema debe impedir validar entradas desde dispositivos no autorizados.
- RF-75: El sistema debe permitir asignar funcionarios de validacion a sectores durante un evento.
- RF-76: El sistema debe registrar los sectores asignados a cada funcionario para cada evento.
- RF-77: El sistema debe permitir verificar que un funcionario haya validado entradas en todos los sectores a los que fue asignado durante un evento.

### Validacion de accesos

- RF-78: El sistema debe permitir escanear entradas dinamicas.
- RF-79: El sistema debe verificar la validez del token o QR activo al momento del escaneo.
- RF-79.1: El sistema debe permitir ingreso manual del token QR cuando el navegador no soporte escaneo por camara o el permiso de camara no este disponible.
- RF-79.2: El sistema debe detener el scanner al detectar un QR y procesar un unico token por intento de validacion.
- RF-80: El sistema debe verificar que la entrada corresponda al evento que se esta validando.
- RF-81: El sistema debe verificar que la entrada corresponda a un sector habilitado del evento.
- RF-82: El sistema debe verificar que la entrada no haya sido consumida previamente.
- RF-83: El sistema debe registrar la fecha y hora de cada acceso validado.
- RF-84: El sistema debe registrar la entrada validada.
- RF-85: El sistema debe registrar el codigo o token aceptado durante la validacion.
- RF-86: El sistema debe registrar la identidad del funcionario que realizo la validacion.
- RF-87: El sistema debe registrar el dispositivo utilizado durante la validacion.
- RF-88: El sistema debe registrar el evento asociado al acceso.
- RF-89: El sistema debe registrar el sector asociado al acceso.
- RF-90: El sistema debe marcar la entrada como consumida de forma irreversible luego de una validacion exitosa.
- RF-91: El sistema debe impedir validar entradas ya consumidas.

### Reportes, consultas y estadisticas

- RF-92: El sistema debe permitir listar las compras efectuadas por cada usuario.
- RF-93: El sistema debe permitir listar las transferencias efectuadas por cada usuario.
- RF-94: El sistema debe permitir listar las entradas actualmente asignadas a cada usuario.
- RF-95: El sistema debe permitir visualizar los eventos en los que se vendieron mas entradas.
- RF-96: El sistema debe permitir visualizar el ranking de usuarios mayores compradores de entradas.

---

## 6. Requerimientos no funcionales

- RNF-1: La aplicacion debe operar bajo arquitectura cliente/servidor.
- RNF-2: La base de datos debe soportar SQL.
- RNF-3: La base de datos debe residir en Linux.
- RNF-4: La implementacion de la base de datos debe realizarse en SQL.
- RNF-5: La aplicacion debe soportar multiples usuarios concurrentes.
- RNF-6: La aplicacion debe ser extensible para futuras funcionalidades.
- RNF-7: El codigo fuente debe mantenerse en un repositorio publico de GitHub.
- RNF-8: La aplicacion debe desarrollarse en un lenguaje soportado por .NET o en Java, salvo aprobacion previa de la catedra.
- RNF-9: La base de datos elegida debe estar justificada.
- RNF-10: El ejecutable debe incluir documentacion suficiente para comprender su funcionamiento y ejecucion.
- RNF-11: Si existen diferencias entre la implementacion documentada y el ejecutable, deben documentarse en un anexo.
- RNF-12: Las credenciales y contrasenas deben almacenarse de forma segura.
- RNF-13: La solucion debe mantener integridad historica de ventas, transferencias, titulares y validaciones.
- RNF-14: La solucion debe preservar consistencia ante operaciones concurrentes de compra, transferencia y validacion.

---

