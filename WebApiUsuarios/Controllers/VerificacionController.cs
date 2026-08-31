using Alexa.DAL;
using Alexa.DAL.Seguridad;
using Alexa.DTOs;
using Alexa.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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
            //var usuarioValido1 = await (from cust in context.Usuario
            //                           join ord in context.relUsuarioRol
            //                           on new { a = cust.UsuarioId } equals new { a = ord.UsuarioId } into ps
            //                           from ord in ps.DefaultIfEmpty()
            //                           where cust.UsuarioId == utilisatrice && cust.Password == passe && cust.Activo == true && ord.RolId == "SupervisorCenso"
            //                           select cust).ToListAsync();
            var usuarioValido = await (from u in context.Usuario
                                       join r in context.relUsuarioRol on u.UsuarioId equals r.UsuarioId
                                       where u.UsuarioId == utilisatrice && u.Password == passe && u.Activo == true && r.RolId == "SupervisorCenso"
                                       select u).AnyAsync();


            if (!usuarioValido)
            {
                return NotFound(new { Retorno = -1, Mensaje = "Credenciales de logueo incorrectas", URL = "error" });
            }

            // 3. Generar token
            var sToken = ConstruirToken(utilisatrice);

            // 4. Ejecutar procedimiento almacenado sde.spUsuarioValidar
            var olista = new List<object>();

            using (var cmd = context.Database.GetDbConnection().CreateCommand())
            {
                cmd.CommandText = "EXEC sde.spUsuarioValidar @UsuarioId, @Password, @sistema, @session, @ip, @VIN";
                cmd.Parameters.Add(new SqlParameter("@UsuarioId", utilisatrice));
                cmd.Parameters.Add(new SqlParameter("@Password", passe));
                cmd.Parameters.Add(new SqlParameter("@VIN", sToken.Token));

                if (cmd.Connection.State != ConnectionState.Open)
                {
                    await cmd.Connection.OpenAsync();
                }

                using (var rdr = await cmd.ExecuteReaderAsync())
                {
                    while (await rdr.ReadAsync())
                    {
                        if (rdr[0] != null && rdr[0] != DBNull.Value)
                        {
                            string jsonString = rdr[0].ToString() ?? "[]";
                            dynamic catalogos = JsonConvert.DeserializeObject(jsonString);
                            olista.Add(catalogos);
                        }
                    }
                }
            }

            return Ok(olista);

            //else
            //{
            //        var sToken = ConstruirToken(utilisatrice);
            //        var olista = new List<object>();
            //        DbCommand cmd;
            //        DbDataReader rdr;
            //        string sql = "EXEC sde.spUsuarioValidar @UsuarioId, @Password, @VIN";
            //        List<SqlParameter> parms = new List<SqlParameter>
            //        { 
            //            // Create parameters    
            //            new SqlParameter { ParameterName = "@UsuarioId", Value = utilisatrice },
            //            new SqlParameter { ParameterName = "@Password", Value = passe },
            //            new SqlParameter { ParameterName = "@VIN", Value = sToken.Token }
            //        };
            //        cmd = context.Database.GetDbConnection().CreateCommand();
            //        cmd.Parameters.AddRange(parms.ToArray());
            //        cmd.CommandText = sql;

            //        // Open database connection  
            //        context.Database.OpenConnection();

            //        // Create a DataReader  
            //        rdr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);

            //        while (rdr.Read())
            //        {
            //            string jsonString = "";
            //            if (rdr[0] != null)
            //            {
            //                jsonString = rdr[0].ToString() ?? "[]";
            //                dynamic catalogos = JsonConvert.DeserializeObject(jsonString);
            //                olista.Add(catalogos);
            //            };
            //        }
            //        rdr.Close();
            //        return Ok(olista);
            //}
        }

        [HttpPost("RefreshToken")]
        [Authorize]
        public IActionResult Refresh()
        {
            var utilisatrice = User.FindFirst("UTILISATRICE")?.Value;
            if (string.IsNullOrEmpty(utilisatrice)) return Unauthorized();

            var token = ConstruirToken(utilisatrice);
            return Ok(token);
        }

        private RespuestaAutenticacion ConstruirToken(string utilisatrice)
        {
            var claims = new List<Claim>
            {
                new Claim("UTILISATRICE", utilisatrice),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expirationHours = configuration.GetValue<int>("JWT:ExpiresInHours", 12);
            var expiracion = DateTime.UtcNow.AddHours(expirationHours);

            var securityToken = new JwtSecurityToken(
                issuer: configuration["JWT:Issuer"],
                audience: configuration["JWT:Audience"],
                claims: claims,
                expires: expiracion,
                signingCredentials: creds
            );

            return new RespuestaAutenticacion()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                Expiracion = expiracion
            };
        }

        private RespuestaAutenticacion ConstruirToken1(string utilisatrice)
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
