using Alexa;
using Alexa.Filters;
using Alexa.Filtros;
using Alexa.Middleware;
using Alexa.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// --- Configuration ---
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// --- Services ---
builder.Services.AddControllers(opciones =>
{
    opciones.Filters.Add(typeof(FiltroDeExcepcion));
    opciones.Filters.Add<NoCacheAttribute>();
}).AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles).AddNewtonsoftJson();

builder.Services.AddControllers().AddXmlDataContractSerializerFormatters();

// DB Contexts
builder.Services.AddDbContext<SecondaryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Externo")));

builder.Services.AddDbContext<CatalogsDbContext>(options =>
   options.UseSqlServer(builder.Configuration.GetConnectionString("cnnCatalogos")));

builder.Services.AddDbContext<EinkommenDbContext>(options =>
   options.UseSqlServer(builder.Configuration.GetConnectionString("cnnEinkommen")));

builder.Services.AddDbContext<CenagroDbContext>(options =>
  options.UseSqlServer(builder.Configuration.GetConnectionString("Cenagro")));

builder.Services.AddDbContext<SisanomDbContext>(options =>
  options.UseSqlServer(builder.Configuration.GetConnectionString("Sisanom")));

builder.Services.AddDbContext<CapacitacionDbContext>(options =>
   options.UseSqlServer(builder.Configuration.GetConnectionString("cnnCapacitacion")));

builder.Services.AddDbContext<ArtemisaDbContext>(options =>
  options.UseSqlServer(builder.Configuration.GetConnectionString("Boleta")));

builder.Services.AddDbContext<IpcDbContext>(options =>
   options.UseSqlServer(builder.Configuration.GetConnectionString("CIPC")));

builder.Services.AddDbContext<IppDeskDbContext>(options =>
   options.UseSqlServer(builder.Configuration.GetConnectionString("CIPPDesk")));

builder.Services.AddDbContext<IppDbContext>(options =>
   options.UseSqlServer(builder.Configuration.GetConnectionString("CIPP")));
// options.UseSqlServer(builder.Configuration.GetConnectionString("CIPP"), o => o.UseCompatibilityLevel(100)));

builder.Services.AddDbContextFactory<IppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CIPP")));

builder.Services.AddDbContextFactory<LocalleDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Localle")));

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones => opciones.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])),
        ClockSkew = TimeSpan.Zero
    });

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Alexa",
        Version = "v1",
        Description = "Proyecto de Servicios en Linea de Bases de Datos",
        Contact = new OpenApiContact
        {
            Email = "Admin@inide.gob.ni",
            Name = "INIDE",
            Url = new Uri("https://www.inide.gob.ni/")
        },
        License = new OpenApiLicense
        {
            Name = "Permisos de Uso",
            Url = new Uri("https://inide/license")
        },
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
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
            new string[]{ }
        }
    });
});

builder.Services.AddAutoMapper(typeof(Program));

// CORS
builder.Services.AddCors(options =>
{
    //options.AddPolicy("AllowAllOriginsWithCredentials", policyBuilder =>
    //{
    //    policyBuilder
    //        .SetIsOriginAllowed(origin => true)
    //        .AllowAnyHeader()
    //        .AllowAnyMethod()
    //        .AllowCredentials();
    //});
    options.AddPolicy("AllowDynamic", policyBuilder =>
    {
        policyBuilder
            .WithOrigins("https://nicboleta.inide.gob.ni", "https://datos-ipp.inide.gob.ni", "https://ipp.inide.gob.ni", "https://ippcapacitacion.inide.gob.ni", "http://127.0.0.1:5500") //  ✅ Origen explícito             
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();                              // ✅ Ahora sí funciona
    });
});

//HttpClient
builder.Services.AddHttpClient();

// Kestrel Configuration (replacing manual app.Use to remove headers)
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.AddServerHeader = false;
});

var app = builder.Build();

// --- HTTP Request Pipeline ---

app.UseLogueaRespuestaErrorHTTP();

if (app.Environment.IsProduction())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "INIDE v1"));
}

app.UseHttpsRedirection();

app.UseRouting();

//app.UseCors("AllowAllOriginsWithCredentials");
app.UseCors("AllowDynamic");

app.UseAuthorization();

app.MapControllers();

app.Run();
