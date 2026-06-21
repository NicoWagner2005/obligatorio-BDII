# obligatorio-BDII

Proyecto del obligatorio de Bases de Datos II: sistema de ticketing para el
Mundial 2026 con registro de usuarios, gestion de estadios/eventos, ventas,
transferencias, QR dinamico y validacion de accesos.

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

## Tests

```bash
dotnet test src/TicketingMundialUCU.slnx
```
