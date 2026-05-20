using AppWebSistemaComandasDigital.Data;
using AppWebSistemaComandasDigital.Helpers;
using AppWebSistemaComandasDigital.Middlewares;
using AppWebSistemaComandasDigital.RealTime;
using AppWebSistemaComandasDigital.Repositories;
using AppWebSistemaComandasDigital.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── Base de datos ─────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
        npgsql.CommandTimeout(60);
    }));

// ── CORS para app móvil Android ───────────────────────────────────────
builder.Services.AddCors(options =>
    options.AddPolicy("AppMovil", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()));

// ── JWT Authentication ────────────────────────────────────────────────
var jwtConfig = builder.Configuration.GetSection("Jwt");
var jwtKey    = Encoding.UTF8.GetBytes(jwtConfig["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey         = new SymmetricSecurityKey(jwtKey),
        ValidateIssuer           = true,
        ValidIssuer              = jwtConfig["Issuer"],
        ValidateAudience         = true,
        ValidAudience            = jwtConfig["Audience"],
        ValidateLifetime         = true,
        ClockSkew                = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = ctx =>
        {
            if (ctx.Request.Cookies.TryGetValue("jwt", out var token))
                ctx.Token = token;
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SoloAdmin",  p => p.RequireClaim("rol_nombre", "Admin"));
    options.AddPolicy("AdminOMozo", p => p.RequireClaim("rol_nombre", "Admin", "Mozo"));
    options.AddPolicy("Cocina",     p => p.RequireClaim("rol_nombre", "Admin", "Cocina"));
});

builder.Services.AddSignalR();
builder.Services.AddControllersWithViews();

// ── Repositorios ──────────────────────────────────────────────────────
builder.Services.AddScoped<IMesaRepository,      MesaRepository>();
builder.Services.AddScoped<IPlatoRepository,     PlatoRepository>();
builder.Services.AddScoped<IPedidoRepository,    PedidoRepository>();
builder.Services.AddScoped<IUsuarioRepository,   UsuarioRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();

// ── Servicios ─────────────────────────────────────────────────────────
builder.Services.AddScoped<MesaService>();
builder.Services.AddScoped<PlatoService>();
builder.Services.AddScoped<PedidoService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CategoriaService>();
builder.Services.AddScoped<ConfiguracionRestauranteService>();

// ── Helpers ───────────────────────────────────────────────────────────
builder.Services.AddScoped<JwtHelper>();

// ═════════════════════════════════════════════════════════════════════
var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}
// ═════════════════════════════════════════════════════════════════════

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    app = "AppWebSistemaComandasDigital",
    environment = app.Environment.EnvironmentName
}));

app.UseStaticFiles();
app.UseRouting();

// CORS antes de Auth — necesario para que la app móvil pueda enviar requests
app.UseCors("AppMovil");

app.UseExceptionMiddleware();
app.UseRequestLogging();
app.UseJwtMiddleware();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.MapHub<PedidoHub>("/hubs/pedidos");

app.Run();
