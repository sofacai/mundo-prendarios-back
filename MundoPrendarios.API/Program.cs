using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MundoPrendarios.Core.Interfaces;
using MundoPrendarios.Core.Mapping;
using MundoPrendarios.Core.Services.Implementaciones;
using MundoPrendarios.Core.Services.Interfaces;
using MundoPrendarios.Infrastructure.Data;
using MundoPrendarios.Infrastructure.Repositories;
using MundoPrendarios.Infrastructure.Services;
using System.Text;


var builder = WebApplication.CreateBuilder(args);
        
// Configuraci�n de la base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Agregar servicios al contenedor
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MundoPrendarios API", Version = "v1" });

    // Configuraci�n para JWT en Swagger
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

// Configuraci�n de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder =>
        {
            builder
                .WithOrigins("http://localhost:8100") // Tu URL de desarrollo de Angular
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
});

// Configuraci�n de JWT
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
builder.Services.AddScoped<IOperacionRepository, OperacionRepository>();



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



// Agregar AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));
// A�adir los dem�s servicios aqu� a medida que se vayan implementando
// builder.Services.AddScoped<ICanalService, CanalService>();
// builder.Services.AddScoped<ISubcanalService, SubcanalService>();
// etc.

var app = builder.Build();

// Configure el pipeline de solicitudes HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigin");


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Configuraci�n para migraci�n autom�tica y seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();
        // Si necesitas datos iniciales, puedes agregar c�digo aqu� para sembrar la base de datos
        await SeedData.SeedAsync(context, loggerFactory);

        // Agrega aqu� la llamada al m�todo para actualizar la contrase�a
        await SeedData.ActualizarContrase�aAdmin(context);
    }
    catch (Exception ex)
    {
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(ex, "Un error ocurri� durante la migraci�n");
    }
}


app.Run();