using Microsoft.AspNetCore.Mvc;
using Alexa.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.AspNetCore.DataProtection;
using Alexa.Servicios;
using Alexa.DAL;
using Alexa.DAL.Seguridad;
using Newtonsoft.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Collections;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Alexa.Controllers
{
    [ApiController]
    [Route("endpoint/verifica")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class VerificacionController : Controller
    {
        private readonly SecondaryDbContext context;
        private readonly IConfiguration configuration;

        public VerificacionController(SecondaryDbContext context,
            IConfiguration configuration)
        {
            this.context = context;
            this.configuration = configuration;
        }

        [HttpPost("Connecter")]
        public async Task<IActionResult> Connecter(string utilisatrice, string passe)
        {
            var q = await  (from cust in context.Usuario
                    join ord in context.relUsuarioRol 
                    on new { a = cust.UsuarioId } equals new { a = ord.UsuarioId } into ps
                    from ord in ps.DefaultIfEmpty()
                    where cust.UsuarioId == utilisatrice && cust.Password == passe && cust.Activo == true && ord.RolId == "SupervisorCenso"
                    select cust).ToListAsync();

            if (q.Count == 0)
            {
                return NotFound(new { Retorno = -1, Mensaje = "Credenciales de logueo incorrectas", URL = "error" });
            }
            else
            {
                    var sToken = ConstruirToken(utilisatrice);
                    var olista = new List<object>();
                    DbCommand cmd;
                    DbDataReader rdr;
                    string sql = "EXEC sde.spUsuarioValidar @UsuarioId, @Password, @VIN";
                    List<SqlParameter> parms = new List<SqlParameter>
                    { 
                        // Create parameters    
                        new SqlParameter { ParameterName = "@UsuarioId", Value = utilisatrice },
                        new SqlParameter { ParameterName = "@Password", Value = passe },
                        new SqlParameter { ParameterName = "@VIN", Value = sToken.Token }
                    };
                    cmd = context.Database.GetDbConnection().CreateCommand();
                    cmd.Parameters.AddRange(parms.ToArray());
                    cmd.CommandText = sql;

                    // Open database connection  
                    context.Database.OpenConnection();

                    // Create a DataReader  
                    rdr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);

                    while (rdr.Read())
                    {
                        string jsonString = "";
                        if (rdr[0] != null)
                        {
                            jsonString = rdr[0].ToString() ?? "[]";
                            dynamic catalogos = JsonConvert.DeserializeObject(jsonString);
                            olista.Add(catalogos);
                        };
                    }
                    rdr.Close();
                    return Ok(olista);
            }
        }

        private RespuestaAutenticacion ConstruirToken(string utilisatrice)
        {
            var claims = new List<Claim>();
            claims = new List<Claim>()
                {
                    new Claim("UTILISATRICE", utilisatrice)
                };

            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["llavejwt"]));
            var creds = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

            var expiracion = DateTime.UtcNow.AddHours(12);

            var securityToken = new JwtSecurityToken(issuer: null, audience: null, claims: claims,
                expires: expiracion, signingCredentials: creds);

            return new RespuestaAutenticacion()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                Expiracion = expiracion
            };
        }

        private string DecodeFrom64(string cadena)
        {
            return System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(cadena));
        }

        private string jsonRetorno(int Retorno, string Mensaje, bool Resultado, string Valor = "NINGUNO")
        {
            var jsonMensaje = "{" + "\"" + "Mensaje" + "\"" + ": " + "\"" + Mensaje + "\"" + ", " + "\"" + "Retorno" + "\"" + ": " + "\"" + Retorno.ToString() + "\"" + ", " + "\"" + "Resultado" + "\"" + ": " + "\"" + Resultado.ToString().ToLower() + "\"" + ", " + "\"" + "Valor" + "\"" + ": " + Valor + "}";
            return jsonMensaje;
        }
    }
}
