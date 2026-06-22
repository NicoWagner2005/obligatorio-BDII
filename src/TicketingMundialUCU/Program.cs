using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TicketingMundialUCU.Components;
using TicketingMundialUCU.Components.Account;
using TicketingMundialUCU.Data;
using TicketingMundialUCU.Data.Daos;
using TicketingMundialUCU.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IUserDao, UserDao>();
builder.Services.AddScoped<IUserPhoneDao, UserPhoneDao>();
builder.Services.AddScoped<IAdministratorJurisdictionDao, AdministratorJurisdictionDao>();
builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();

builder.Services.AddScoped<UserRegistrationService>();
builder.Services.AddScoped<UserPhoneService>();
builder.Services.AddScoped<AdministratorJurisdictionService>();

builder.Services.AddScoped<IEstadioDao, EstadioDao>();
builder.Services.AddScoped<IEventoDao, EventoDao>();
builder.Services.AddScoped<IVentaDao, VentaDao>();
builder.Services.AddScoped<IEntradaDao, EntradaDao>();
builder.Services.AddScoped<ITokenQrDao, TokenQrDao>();

builder.Services.AddScoped<IFuncionarioDao, FuncionarioDao>();
builder.Services.AddScoped<ITransferenciaDao, TransferenciaDao>();
builder.Services.AddScoped<IAuditoriaDao, AuditoriaDao>();

builder.Services.AddScoped<EstadioService>();
builder.Services.AddScoped<EventoService>();
builder.Services.AddScoped<VentaService>();
builder.Services.AddScoped<FuncionarioService>();
builder.Services.AddScoped<TransferenciaService>();
builder.Services.AddScoped<AuditoriaService>();
builder.Services.AddScoped<TokenQrService>();
builder.Services.AddScoped<QrCodeService>();
builder.Services.Configure<TokenQrRefreshOptions>(
    builder.Configuration.GetSection("TokenQrRefresh"));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHostedService<TokenQrRefreshWorker>();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
