# Requerimientos

## Actores
- Usuario general
- Funcionario de validación
- Administrador

## Business Use Cases, BUC

### Comunes
- BUC-1: Gestionar cuenta de usuario

### Usuario general
- BUC-2: Gestionar compra de entradas
- BUC-3: Gestionar posesión y transferencia de entradas

### Funcionario de validación
- BUC-4: Gestionar validación de entradas

### Administrador
- BUC-5: Gestionar estadios
- BUC-6: Gestionar eventos

## Product Use Cases, PUC

### BUC-1: Gestionar cuenta de usuario
- PUC-1: Registrarse
- PUC-2: Iniciar sesión
- PUC-3: Cerrar sesión

### BUC-2: Gestionar compra de entradas
- PUC-4: Comprar entrada

### BUC-3: Gestionar posesión y transferencia de entradas
- PUC-5: Recibir entrada transferida
- PUC-6: Transferir entrada
- PUC-7: Ver mis entradas

### BUC-4: Gestionar validación de entradas
- PUC-8: Validar entrada

### BUC-5: Gestionar estadios
- PUC-9: Definir estadio
- PUC-10: Modificar estadio
- PUC-11: Definir sectores

### BUC-6: Gestionar eventos
- PUC-14: Programar evento
- PUC-15: Modificar evento

### BUC-7: Gestionar seguridad 
- PUC-16: Generar token
- PUC-17: Registrar dispositivos de escaneo
- PUC-18: 

## Requerimientos Funcionales

### PUC-11: Definir sectores
- RF-1: El sistema debe permitir al administrador configurar el aforo máximo de un sector.
- RF-2: El sistema debe permitir al administrador configurar el costo de las entradas de un sector.

### PUC-14: Programar evento
- RF-3: El sistema debe permitir al administrador definir un equipo local y otro visitante.
- RF-4: El sistema debe permitir al administrador vincular el evento a un estadio específico.
- RF-5: El sistema debe permitir al administrador asignar una fecha y hora exacta al evento.
- RF-6: El sistema debe permitir al administrador habilitar uno o más sectores para un evento.

### PUC-4: Comprar entrada
- RF-7: El sistema debe permitir al usuario comprar múltiples entradas en una única venta.
- RF-8: El sistema debe permitir al usuario incluir entradas de distintos sectores en una misma venta.
- RF-9: El sistema debe generar un identificador único para cada entrada.
- RF-10: El sistema debe asignar inicialmente las entradas al usuario comprador.

### PUC-6: Transferir entrada
- RF-11: El sistema debe permitir al usuario iniciar la transferencia de una entrada a otro usuario.
- RF-12: El sistema debe verificar que la entrada pueda ser transferida.
- RF-13: El sistema debe impedir transferir una entrada ya validada.
- RF-14: El sistema debe impedir transferir una entrada que ya fue transferida 3 veces.
- RF-15: El sistema debe permitir al usuario destinatario aceptar la transferencia.
- RF-16: El sistema debe cambiar el propietario de la entrada al aceptarse la transferencia.
- RF-17: El sistema debe registrar cada transferencia realizada.

### PUC-16: Gestionar token
- RF-18: El sistema debe generar un token y asignárselo a la entrada.
- RF-19: El sistema debe regenerar el token de cada entrada cada 30 segundos.

### PUC-17: Registrar dispositivos de escaneo
- RF-20: El sistema debe permitir al administrador registrar dispositivos de escaneo.


- RF-21: El sistema debe permitir al administrador ver los dispoitivos de eescaneo registrados.

## Requerimientos No Funcionales
