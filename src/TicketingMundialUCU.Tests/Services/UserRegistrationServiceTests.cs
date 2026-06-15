using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using NSubstitute;
using TicketingMundialUCU.Data;
using TicketingMundialUCU.Data.Repositories;
using TicketingMundialUCU.Services;

namespace TicketingMundialUCU.Tests.Services;

public sealed class UserRegistrationServiceTests
{
    [Fact]
    public async Task RegistrarUsuario_con_rol_invalido_no_crea_el_usuario()
    {
        await using var context = CrearContexto();
        var data = CrearDatos(role: "superadmin");

        var result = await context.Service.RegisterUserAsync(data);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Code == "InvalidRole");
        Assert.Null(await context.UserManager.FindByEmailAsync(data.Email));
        await context.UserRepository.DidNotReceiveWithAnyArgs().CreateAsync(
            default!,
            default!,
            default!,
            default!,
            default!,
            default!,
            default!,
            default!,
            default!,
            default!,
            default);
    }

    [Fact]
    public async Task RegistrarUsuario_con_password_invalida_no_crea_el_perfil()
    {
        await using var context = CrearContexto();
        var data = CrearDatos(password: "corta");

        var result = await context.Service.RegisterUserAsync(data);

        Assert.False(result.Succeeded);
        await context.UserRepository.DidNotReceiveWithAnyArgs().CreateAsync(
            default!,
            default!,
            default!,
            default!,
            default!,
            default!,
            default!,
            default!,
            default!,
            default!,
            default);
    }

    [Theory]
    [InlineData(UserRoles.General)]
    [InlineData(UserRoles.Funcionario)]
    [InlineData(UserRoles.Administrador)]
    public async Task RegistrarUsuario_valido_crea_perfil_y_asigna_el_rol(string role)
    {
        await using var context = CrearContexto();
        var data = CrearDatos(role: role);

        var result = await context.Service.RegisterUserAsync(data);

        Assert.True(result.Succeeded);
        var user = await context.UserManager.FindByEmailAsync(data.Email);
        Assert.NotNull(user);
        Assert.True(await context.UserManager.IsInRoleAsync(user, role));
        await context.UserRepository.Received(1).CreateAsync(
            user.Id,
            data.NroDocumento,
            data.TipoDocumento,
            data.PaisDocumento,
            data.PaisDireccion,
            data.Localidad,
            data.Calle,
            data.NroDireccion,
            data.CodigoPostal,
            role,
            data.PaisSedeAsignado);
        await context.UserPhoneRepository.Received(1).AddAsync(user.Id, data.Telefono);
    }

    [Fact]
    public async Task RegistrarUsuario_sin_telefono_no_agrega_un_telefono_vacio()
    {
        await using var context = CrearContexto();
        var data = CrearDatos(telefono: " ");

        var result = await context.Service.RegisterUserAsync(data);

        Assert.True(result.Succeeded);
        await context.UserPhoneRepository.DidNotReceive().AddAsync(
            Arg.Any<string>(),
            Arg.Any<string>());
    }

    [Fact]
    public async Task RegistrarAdministrador_sin_pais_sede_no_crea_el_usuario()
    {
        await using var context = CrearContexto();
        var data = CrearDatos(role: UserRoles.Administrador) with
        {
            PaisSedeAsignado = null
        };

        var result = await context.Service.RegisterUserAsync(data);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Code == "InvalidAdministratorCountry");
        Assert.Null(await context.UserManager.FindByEmailAsync(data.Email));
    }

    [Fact]
    public async Task RegistrarAdministrador_con_pais_no_catalogado_no_crea_el_usuario()
    {
        await using var context = CrearContexto();
        var data = CrearDatos(
            role: UserRoles.Administrador,
            paisSedeAsignado: "Uruguay");

        var result = await context.Service.RegisterUserAsync(data);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Code == "InvalidAdministratorCountry");
        Assert.Null(await context.UserManager.FindByEmailAsync(data.Email));
    }

    [Theory]
    [InlineData("uq_usuarios_documento", "Ya existe un usuario registrado con ese número de documento.")]
    [InlineData("uq_usuarios_email", "Ya existe un usuario registrado con ese email.")]
    [InlineData("uq_otro", "Ya existe un usuario registrado con esos datos.")]
    public async Task RegistrarUsuario_con_dato_duplicado_elimina_identity_y_explica_el_error(
        string constraintName,
        string mensajeEsperado)
    {
        await using var context = CrearContexto();
        var data = CrearDatos();
        context.UserRepository
            .CreateAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>())
            .Returns(Task.FromException(CrearUniqueViolation(constraintName)));

        var result = await context.Service.RegisterUserAsync(data);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Description == mensajeEsperado);
        Assert.Null(await context.UserManager.FindByEmailAsync(data.Email));
    }

    private static RegistrationTestContext CrearContexto()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase($"registration-tests-{Guid.NewGuid()}"));
        services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        return new RegistrationTestContext(services.BuildServiceProvider());
    }

    private static UserRegistrationData CrearDatos(
        string role = UserRoles.General,
        string password = "Password1!",
        string telefono = "+59899123456",
        string? paisSedeAsignado = null) =>
        new(
            "persona@ucu.edu.uy",
            password,
            "12345678",
            "CI",
            "Uruguay",
            "Uruguay",
            "Montevideo",
            "18 de Julio",
            "1234",
            "11200",
            telefono,
            role,
            role == UserRoles.Administrador
                ? paisSedeAsignado ?? "México"
                : paisSedeAsignado);

    private static PostgresException CrearUniqueViolation(string constraintName) =>
        new(
            "duplicate key value violates unique constraint",
            "ERROR",
            "ERROR",
            PostgresErrorCodes.UniqueViolation,
            null!,
            null!,
            0,
            0,
            null!,
            null!,
            "public",
            "usuarios",
            null!,
            null!,
            constraintName,
            "nbtinsert.c",
            "664",
            "_bt_check_unique");

    private sealed class RegistrationTestContext : IAsyncDisposable
    {
        private readonly ServiceProvider _provider;
        private readonly AsyncServiceScope _scope;

        public RegistrationTestContext(ServiceProvider provider)
        {
            _provider = provider;
            _scope = provider.CreateAsyncScope();

            UserManager = _scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = _scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userStore = _scope.ServiceProvider.GetRequiredService<IUserStore<ApplicationUser>>();

            UserRepository = Substitute.For<IUserRepository>();
            UserPhoneRepository = Substitute.For<IUserPhoneRepository>();
            var currentUser = Substitute.For<ICurrentUserContext>();
            var jurisdictionRepository = Substitute.For<IAdministratorJurisdictionRepository>();
            jurisdictionRepository.CountryExistsAsync("México").Returns(true);
            var jurisdictionService = new AdministratorJurisdictionService(
                jurisdictionRepository,
                currentUser);
            Service = new UserRegistrationService(
                UserManager,
                roleManager,
                userStore,
                UserRepository,
                UserPhoneRepository,
                jurisdictionService);
        }

        public UserManager<ApplicationUser> UserManager { get; }
        public IUserRepository UserRepository { get; }
        public IUserPhoneRepository UserPhoneRepository { get; }
        public UserRegistrationService Service { get; }

        public async ValueTask DisposeAsync()
        {
            await _scope.DisposeAsync();
            await _provider.DisposeAsync();
        }
    }
}
