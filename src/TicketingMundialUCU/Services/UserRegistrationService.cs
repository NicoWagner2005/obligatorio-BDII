using Microsoft.AspNetCore.Identity;
using Npgsql;
using TicketingMundialUCU.Data;
using TicketingMundialUCU.Data.Daos;

namespace TicketingMundialUCU.Services;

public sealed class UserRegistrationService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IUserStore<ApplicationUser> userStore,
    IUserDao userDao,
    IUserPhoneDao userPhoneDao,
    AdministratorJurisdictionService jurisdictionService)
{
    public async Task<IdentityResult> RegisterUserAsync(UserRegistrationData registrationData)
    {
        var user = CreateUser();
        if (!UserRoles.IsValid(registrationData.Role))
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "InvalidRole",
                Description = "El rol seleccionado no es válido."
            });
        }

        var countryResult = await jurisdictionService.ValidateRegistrationCountryAsync(
            registrationData.Role,
            registrationData.PaisSedeAsignado);
        if (!countryResult.Succeeded)
        {
            return countryResult;
        }

        await userStore.SetUserNameAsync(user, registrationData.Email, CancellationToken.None);
        var emailStore = GetEmailStore();
        await emailStore.SetEmailAsync(user, registrationData.Email, CancellationToken.None);

        var result = await userManager.CreateAsync(user, registrationData.Password);
        if (!result.Succeeded)
        {
            return result;
        }

        try
        {
            await userDao.CreateAsync(
                user.Id,
                registrationData.NroDocumento,
                registrationData.TipoDocumento,
                registrationData.PaisDocumento,
                registrationData.PaisDireccion,
                registrationData.Localidad,
                registrationData.Calle,
                registrationData.NroDireccion,
                registrationData.CodigoPostal,
                registrationData.Role,
                registrationData.PaisSedeAsignado);

            if (!string.IsNullOrWhiteSpace(registrationData.Telefono))
            {
                await userPhoneDao.AddAsync(user.Id, registrationData.Telefono);
            }

            var roleResult = await EnsureRoleAsync(registrationData.Role);
            if (!roleResult.Succeeded)
            {
                await userManager.DeleteAsync(user);
                return roleResult;
            }

            var addToRoleResult = await userManager.AddToRoleAsync(user, registrationData.Role);
            if (!addToRoleResult.Succeeded)
            {
                await userManager.DeleteAsync(user);
                return addToRoleResult;
            }
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            await userManager.DeleteAsync(user);

            return IdentityResult.Failed(new IdentityError
            {
                Code = ex.ConstraintName ?? PostgresErrorCodes.UniqueViolation,
                Description = GetUniqueViolationMessage(ex)
            });
        }

        return result;
    }

    private async Task<IdentityResult> EnsureRoleAsync(string role)
    {
        if (await roleManager.RoleExistsAsync(role))
        {
            return IdentityResult.Success;
        }

        return await roleManager.CreateAsync(new IdentityRole(role));
    }

    private static ApplicationUser CreateUser()
    {
        try
        {
            return Activator.CreateInstance<ApplicationUser>();
        }
        catch
        {
            throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor.");
        }
    }

    private IUserEmailStore<ApplicationUser> GetEmailStore()
    {
        if (!userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }

        return (IUserEmailStore<ApplicationUser>)userStore;
    }

    private static string GetUniqueViolationMessage(PostgresException exception)
    {
        return exception.ConstraintName switch
        {
            // The migration names this constraint "uq_usuarios_documento", not after the nro_documento column.
            string constraintName when constraintName.Contains("documento", StringComparison.OrdinalIgnoreCase) =>
                "Ya existe un usuario registrado con ese número de documento.",
            string constraintName when constraintName.Contains("email", StringComparison.OrdinalIgnoreCase) =>
                "Ya existe un usuario registrado con ese email.",
            _ => "Ya existe un usuario registrado con esos datos."
        };
    }
}

public sealed record UserRegistrationData(
    string Email,
    string Password,
    string NroDocumento,
    string TipoDocumento,
    string PaisDocumento,
    string PaisDireccion,
    string Localidad,
    string Calle,
    string NroDireccion,
    string CodigoPostal,
    string Telefono,
    string Role,
    string? PaisSedeAsignado);
