# Obligatorio-BDII

Proyecto del obligatorio de Bases de Datos II: sistema de ticketing para el
Mundial 2026 con registro de usuarios, gestion de estadios/eventos, ventas,
transferencias, QR dinamico y validacion de accesos.

## Scanner QR (aclaracion)

El scanner QR se usa desde la pantalla de funcionario para leer el token dinamico
con la camara del navegador. Si el navegador o el permiso de camara no estan
disponibles, el funcionario puede ingresar el token manualmente; en ambos casos
la validacion definitiva se realiza en el servidor.

## Criterio de base de datos para la entrega

El proyecto separa dos responsabilidades:

- **Identity / autenticacion**: se usa ASP.NET Core Identity como libreria de
  .NET. Sus tablas `AspNet*` y consultas internas no forman parte del SQL propio
  del obligatorio.
- **Dominio del obligatorio**: todo el modelo propio de ticketing queda visible
  en SQL y las operaciones de negocio se implementan con Dapper en los DAOs.

La migracion activa de EF es una unica migracion inicial en
`src/TicketingMundialUCU/Data/Migrations/`. Al iniciar la aplicacion,
`db.Database.MigrateAsync()` aplica esa migracion y crea toda la base desde cero
si el volumen esta vacio.

Las migraciones incrementales anteriores fueron reemplazadas por esta migracion
inicial final para que la demo sea directa y reproducible.

## Ejecucion local

1. Levantar PostgreSQL:

   ```bash
   docker compose up -d db
   ```

2. Ejecutar la aplicacion:

   ```bash
   dotnet run --project src/TicketingMundialUCU/TicketingMundialUCU.csproj
   ```

La aplicacion aplica la migracion inicial al arrancar. En una base vacia crea
Identity y todo el dominio propio automaticamente.

Para reiniciar la base desde cero:

```bash
docker compose down -v
docker compose up -d db
dotnet run --project src/TicketingMundialUCU/TicketingMundialUCU.csproj
```
El archivo .env se incluye en este repositorio únicamente para facilitar la demostración y evaluación de la aplicación. Somos conscientes de que esto constituye una mala práctica desde el punto de vista de la seguridad. En un entorno real, el archivo .env nunca debería versionarse; en su lugar, se debería incluir únicamente un archivo .env.example con las variables de entorno necesarias.

## Tests

```bash
dotnet test src/TicketingMundialUCU.slnx
```

## Documentacion

- Requerimientos: `docs/Requirements.md`
- Modelo entidad-relacion editable: `docs/diagrama-MER-obligatorio.drawio`
- Diagramas para el informe: `docs/Diagramas.md`
