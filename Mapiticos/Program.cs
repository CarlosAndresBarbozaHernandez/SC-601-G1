using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Mapiticos.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Mapiticos.Filtros;
using Mapiticos.Repositorios;
using Mapiticos.Servicios;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false) // aprobación de correo para permitir login (de momento desactivado)
    .AddRoles<IdentityRole>()
    .AddUserManager<GestorUsuarios>() // asigna el rol "Usuario" a toda cuenta nueva
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews(options =>
{
    // Toda la app requiere iniciar sesión, salvo lo marcado con [AllowAnonymous]
    var politica = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(politica));

    // Quien inició sesión pero todavía no eligió su @usuario va a Perfil/ElegirUsuario
    options.Filters.Add<RequiereNombreUsuarioFiltro>();
});

// Repositorios (acceso a datos con ADO.NET)
builder.Services.AddScoped<IPerfilRepositorio, PerfilRepositorio>();
builder.Services.AddScoped<IMapaRepositorio, MapaRepositorio>();
builder.Services.AddScoped<ILugarRepositorio, LugarRepositorio>();

// Servicios
builder.Services.AddScoped<IAlmacenFotos, AlmacenFotosLocal>();

var app = builder.Build();

// Crea la BD y las tablas de Identity si no existen, y luego los roles "Usuario" y "Admin"
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    await InicializadorRoles.CrearRolesAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();