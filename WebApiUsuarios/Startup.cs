using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using System.IO;
using Alexa.Middleware;
using Alexa.Filtros;
using Alexa;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Alexa.Servicios;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]
namespace Alexa
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(opciones =>
            {
                opciones.Filters.Add(typeof(FiltroDeExcepcion));
            }).AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles).AddNewtonsoftJson();

            services.AddControllers().AddXmlDataContractSerializerFormatters();

            services.AddDbContext<SecondaryDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("Externo")));

            services.AddDbContext<CatalogsDbContext>(options =>
               options.UseSqlServer(Configuration.GetConnectionString("cnnCatalogos")));

            services.AddDbContext<EinkommenDbContext>(options =>
               options.UseSqlServer(Configuration.GetConnectionString("cnnEinkommen")));

            services.AddDbContext<CenagroDbContext>(options =>
              options.UseSqlServer(Configuration.GetConnectionString("Cenagro")));

            services.AddDbContext<SisanomDbContext>(options =>
              options.UseSqlServer(Configuration.GetConnectionString("Sisanom")));

            services.AddDbContext<CapacitacionDbContext>(options =>
               options.UseSqlServer(Configuration.GetConnectionString("cnnCapacitacion")));

            services.AddDbContext<ArtemisaDbContext>(options =>
              options.UseSqlServer(Configuration.GetConnectionString("Boleta")));

            services.AddDbContext<IpcDbContext>(options =>
               options.UseSqlServer(Configuration.GetConnectionString("CIPC")));
            //,sqlOptions => sqlOptions.EnableRetryOnFailure(
            //         maxRetryCount: 2, // Número máximo de reintentos
            //         maxRetryDelay: TimeSpan.FromSeconds(10), // Tiempo máximo de espera entre reintentos
            //         errorNumbersToAdd: null // Puedes especificar códigos de error adicionales si es necesario
            //     )

            services.AddDbContext<IppDeskDbContext>(options =>
               options.UseSqlServer(Configuration.GetConnectionString("CIPPDesk")));

            services.AddDbContext<IppDbContext>(options =>
               options.UseSqlServer(Configuration.GetConnectionString("CIPP")));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opciones => opciones.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(Configuration["llavejwt"])),
                    ClockSkew = TimeSpan.Zero
                });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
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

            services.AddAutoMapper(typeof(Startup));

            // Definir una política CORS nombrada
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOriginsWithCredentials", builder =>
                {
                    builder
                        .SetIsOriginAllowed(origin => true) // Permite cualquier origen (cuidado en producción)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials(); // Permite cookies y credenciales
                });
            });

            //services.AddCors();

            //builder.Services.AddCors((options =>
            //{
            //    options.AddPolicy(politicaCors, policy =>
            //    {
            //        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            //        //.WithExposedHeaders(new string[] { "cantidadTotalRegistros" }); 
            //    });
            //}));           

            //app.UseCors(politicaCors);
            //app.Use((corsContext, next) =>
            //{
            //    corsContext.Response.Headers["Access-Control-Allow-Origin"] = "*";
            //    return next.Invoke();
            //});

            //services.AddAuthorization(opciones =>
            //{
            //    opciones.AddPolicy("EsAdmin", politica => politica.RequireClaim("esAdmin"));
            //});
            //
            //services.AddCors(options => {
            //    options.AddPolicy("PermitirCIPCApis", policy => {
            //        policy.WithOrigins("https://appcepov.inide.gob.ni")
            //              .AllowAnyHeader()
            //              .AllowMethods(HttpMethods.Get, HttpMethods.Post);
            //    });
            //});
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            //app.UseLogueaRespuestaHTTp();
            app.UseLogueaRespuestaErrorHTTP();

            if (env.IsProduction())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "INIDE v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            //app.UseCors(x => x
            //   .AllowAnyMethod()
            //   .AllowAnyHeader()
            //   .SetIsOriginAllowed(origin => true) // allow any origin
            //   .AllowCredentials()); // allow credentials

            app.UseCors("AllowAllOriginsWithCredentials"); // Aplicar política CORS

            app.UseAuthorization();


            app.UseEndpoints(endpoint =>
            {
                endpoint.MapControllers();
            });

            //app.UseEndpoints(endpoint => {
            //    endpoint.MapControllerRoute(
            //        name: "default",
            //        pattern: "/Alexa/{controller=Home}/{action=Index}/{id?}");                    
            //});
        }
    }
}
