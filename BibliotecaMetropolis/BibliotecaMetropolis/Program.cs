using BibliotecaMetropolis.Models.DB;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IO;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ----------------------
// Services
// ----------------------
builder.Services.AddControllersWithViews();

// DbContext (lee la cadena desde appsettings.json)
builder.Services.AddDbContext<BibliotecaMetropolisContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// required for session to work
builder.Services.AddDistributedMemoryCache();

// ----------------------
// Persistir claves de DataProtection para que las cookies de sesión sobrevivan reinicios
// ----------------------
var keysFolder = Path.Combine(builder.Environment.ContentRootPath, "DataProtectionKeys");
Directory.CreateDirectory(keysFolder);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysFolder))
    .SetApplicationName("BibliotecaMetropolis");

// Session & HttpContextAccessor
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".BibliotecaMetropolis.Session"; // nuevo nombre forzado
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ----------------------
// JWT configuration: lee la sección "Jwt" en appsettings.json
// ----------------------
var jwtSettings = builder.Configuration.GetSection("Jwt");
var keyString = jwtSettings["Key"];
if (string.IsNullOrEmpty(keyString))
{
    throw new Exception("JWT key no configurada. Añade la sección 'Jwt' en appsettings.json.");
}
var key = Encoding.UTF8.GetBytes(keyString);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.FromMinutes(2)
    };

    // Opcional: permitir que el middleware lea el token desde Session (útil en MVC)
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (string.IsNullOrEmpty(context.Token))
            {
                var tokenFromSession = context.HttpContext.Session.GetString("JWToken");
                if (!string.IsNullOrEmpty(tokenFromSession))
                {
                    context.Token = tokenFromSession;
                }
            }
            return Task.CompletedTask;
        }
    };
});

// ----------------------
// Build app
// ----------------------
var app = builder.Build();

// ----------------------
// Middleware pipeline
// ----------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Orden correcto: Session antes de Authentication/Authorization
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
