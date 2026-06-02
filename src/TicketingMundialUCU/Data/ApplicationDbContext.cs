using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TicketingMundialUCU.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Ignoramos los campos de telefono de Identity porque los telefonos
        // se modelan en la tabla normalizada telefonos_usuario.
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Ignore(user => user.PhoneNumber);
            entity.Ignore(user => user.PhoneNumberConfirmed);
        });
    }
}
