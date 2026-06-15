using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TicketingMundialUCU.Data;

#nullable disable

namespace TicketingMundialUCU.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260615000000_SeedWorldCup2026Teams")]
public partial class SeedWorldCup2026Teams : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
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
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Intentionally left empty: teams may predate this migration or be referenced by events.
    }
}
