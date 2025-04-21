using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MundoPrendarios.Core.Interfaces;
using MundoPrendarios.Core.Mapping;
using MundoPrendarios.Core.Services;
using MundoPrendarios.Core.Services.Implementaciones;
using MundoPrendarios.Core.Services.Interfaces;
using MundoPrendarios.Infrastructure.Data;
using MundoPrendarios.Infrastructure.Repositories;
using MundoPrendarios.Infrastructure.Services;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Configuración de la base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Agregar servicios al contenedor
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MundoPrendarios API", Version = "v1" });

    // Configuración para JWT en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policyBuilder =>
        {
            policyBuilder
                .WithOrigins(
                    "http://localhost:8101",
                      "http://localhost:8100",
                      "http://localhost:4200",
                      "http://192.168.0.8:4200",
                    "https://app.mundoprendario.ar"      
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
});

// Configuración de JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Token:Key"])),
            ValidIssuer = builder.Configuration["Token:Issuer"],
            ValidateIssuer = true,
            ValidateAudience = false
        };
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Registrar repositorios
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ICanalRepository, CanalRepository>();
builder.Services.AddScoped<ISubcanalRepository, SubcanalRepository>();
builder.Services.AddScoped<IPlanRepository, PlanRepository>();
builder.Services.AddScoped<IPlanCanalRepository, PlanCanalRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IOperacionRepository, OperacionRepository>();
builder.Services.AddScoped<IGastoRepository, GastoRepository>();
builder.Services.AddScoped<IReglaCotizacionRepository, ReglaCotizacionRepository>();
builder.Services.AddScoped<IClienteVendorRepository, ClienteVendorRepository>();
builder.Services.AddScoped<ICanalOficialComercialRepository, CanalOficialComercialRepository>();



// Registrar servicios
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<ApplicationDbContext>();
builder.Services.AddScoped<ICanalService, CanalService>();
builder.Services.AddScoped<ISubcanalService, SubcanalService>();
builder.Services.AddScoped<DbContext, ApplicationDbContext>();
builder.Services.AddScoped<IReglaCotizacionService, ReglaCotizacionService>();
builder.Services.AddScoped<IPlanService, PlanService>();
builder.Services.AddScoped<IOperacionService, OperacionService>();
builder.Services.AddScoped<IPlanCanalService, PlanCanalService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IClienteVendorService, ClienteVendorService>();
builder.Services.AddScoped<ICanalOficialComercialService, CanalOficialComercialService>();
builder.Services.AddScoped<IKommoWebhookService, WebhookKommoService>();

builder.Services.AddHttpClient();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers().AddApplicationPart(typeof(TuProyecto.Controllers.KommoController).Assembly);

builder.Services.AddHttpClient("KommoApi").ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    // Deshabilita la validación de certificados solo en desarrollo
#if DEBUG
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
#endif
});



// Agregar AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MundoPrendarios API V1");
        c.RoutePrefix = "swagger"; // Accesible en /swagger
    });
}

app.UseHttpsRedirection();
//app.UseStaticFiles();


app.UseCors("AllowSpecificOrigin");


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Configuración para migración automática y seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();
        // Si necesitas datos iniciales, puedes agregar código aquí para sembrar la base de datos
        await SeedData.SeedAsync(context, loggerFactory);

        // Agrega aquí la llamada al método para actualizar la contraseña
        await SeedData.ActualizarContraseñaAdmin(context);
    }
    catch (Exception ex)
    {
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(ex, "Un error ocurrió durante la migración");
    }
}

app.Run();