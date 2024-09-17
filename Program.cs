using AspNetCoreHero.ToastNotification;
using ManejoClientes.Data;
using ManejoClientes.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Rotativa.AspNetCore;

var builder = WebApplication.CreateBuilder(args);



var politicaUsuariosAutenticados = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser().Build();


// Add services to the container.
builder.Services.AddControllersWithViews(opc =>
{
    opc.Filters.Add(new AuthorizeFilter(politicaUsuariosAutenticados));
});




builder.Services.AddNotyf(c => {
    c.DurationInSeconds = 10;
    c.IsDismissable = true;
    c.Position = NotyfPosition.TopRight;
});




builder.Services.AddDbContext<AppDbcontext>(opc =>
{
    opc.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    
});




builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IServicioUsuarios,ServicioUsuarios>();
builder.Services.AddTransient<IUserStore<Usuario>, UsuarioStore>();
builder.Services.AddIdentityCore<Usuario>(opc =>
{
    opc.Password.RequireNonAlphanumeric = false;
});
builder.Services.AddTransient <SignInManager<Usuario>>();


builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.Secure = CookieSecurePolicy.Always;
});


//1-google auth
builder.Services.AddAuthentication().AddGoogle(opc =>
{
    opc.ClientId = builder.Configuration["GoogleClientId"];
    opc.ClientSecret = builder.Configuration["GoogleSecretId"];
});


//2 Microsoft auth
builder.Services.AddAuthentication(opc =>
{
    opc.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    opc.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    opc.DefaultSignOutScheme = IdentityConstants.ApplicationScheme;
}).AddMicrosoftAccount(opc =>
{
    opc.ClientId = builder.Configuration["MicrosoftClientId"];
    opc.ClientSecret = builder.Configuration["MicrosoftSecretId"];
    
}).AddCookie(opc =>
{
    opc.LoginPath = "/Empleados/Lista";
});





builder.Services.AddIdentity<IdentityUser, IdentityRole>(opciones =>
{
    opciones.SignIn.RequireConfirmedAccount = false;
})
               .AddEntityFrameworkStores<AppDbcontext>()
               .AddDefaultTokenProviders();


builder.Services.PostConfigure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme,
    opciones =>
    {
        opciones.LoginPath = "/Usuarios/login";
        opciones.AccessDeniedPath = "/Usuarios/login";
        opciones.ExpireTimeSpan = TimeSpan.FromDays(1);
        opciones.SlidingExpiration = true;
    });


var app = builder.Build();


//Esto es para la rotativa de poder usar PDF dowload
IWebHostEnvironment env = app.Environment;

RotativaConfiguration.Setup
    (env.WebRootPath, "Rotativa/Windows");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseCookiePolicy();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Empleados}/{action=Lista}/{id?}");



app.Run();
