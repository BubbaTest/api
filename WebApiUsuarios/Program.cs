using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Alexa;

var builder = WebApplication.CreateBuilder(args);
string politicaCors = "permitirTodo";

var startup = new Startup(builder.Configuration);
builder.Services.AddControllers();

startup.ConfigureServices(builder.Services);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors((options =>
{
    options.AddPolicy(politicaCors, policy => { policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        //.WithExposedHeaders(new string[] { "cantidadTotalRegistros" }); 
    });
}));

var app = builder.Build();

var servicioLogger = (ILogger<Startup>)app.Services.GetService(typeof(ILogger<Startup>));

startup.Configure(app, app.Environment, servicioLogger);
app.UseCors(politicaCors);
app.Run();
