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
            }).AddJsonOptions( x => x.JsonSerializerOptions.ReferenceHandler= ReferenceHandler.IgnoreCycles).AddNewtonsoftJson();

            services.AddControllers().AddXmlDataContractSerializerFormatters();
                      
            services.AddDbContext<SecondaryDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("Externo")));

            services.AddDbContext<CatalogsDbContext>(options =>
               options.UseSqlServer(Configuration.GetConnectionString("cnnCatalogos")));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opciones => opciones.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(Configuration["llavejwt"])),
                    ClockSkew= TimeSpan.Zero
                });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Alexa", Version = "v1" });

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

            //services.AddAuthorization(opciones =>
            //{
            //    opciones.AddPolicy("EsAdmin", politica => politica.RequireClaim("esAdmin"));
            //});            
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger) 
        {
            app.UseLogueaRespuestaHTTp();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseSwagger();
                //app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Alexa v1"));
            }

            //produccion
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApiProbando v1"));

            app.UseHttpsRedirection();

            app.UseRouting();
    
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
