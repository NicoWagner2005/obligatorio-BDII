using Microsoft.AspNetCore.Identity;
using Npgsql;
using TicketingMundialUCU.Data;
using TicketingMundialUCU.Data.Repositories;

namespace TicketingMundialUCU.Services;

public sealed class UserRegistrationService(
    UserManager<ApplicationUser> userManager,
    IUserStore<ApplicationUser> userStore,
    UserRepository userRepository,
    UserPhoneRepository userPhoneRepository)
{
    public async Task<IdentityResult> RegisterGeneralUserAsync(GeneralUserRegistrationData registrationData)
    {
        var user = CreateUser();

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
            await userRepository.CreateAsync(
                registrationData.Email,
                registrationData.NroDocumento,
                registrationData.TipoDocumento,
                registrationData.PaisDocumento,
                registrationData.PaisDireccion,
                registrationData.Localidad,
                registrationData.Calle,
                registrationData.NroDireccion,
                registrationData.CodigoPostal);

            if (!string.IsNullOrWhiteSpace(registrationData.Telefono))
            {
                await userPhoneRepository.AddAsync(registrationData.Email, registrationData.Telefono);
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
            string constraintName when constraintName.Contains("nro_documento", StringComparison.OrdinalIgnoreCase) =>
                "Ya existe un usuario registrado con ese número de documento.",
            string constraintName when constraintName.Contains("email", StringComparison.OrdinalIgnoreCase) =>
                "Ya existe un usuario registrado con ese email.",
            _ => "Ya existe un usuario registrado con esos datos."
        };
    }
}

public sealed record GeneralUserRegistrationData(
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
    string Telefono);
