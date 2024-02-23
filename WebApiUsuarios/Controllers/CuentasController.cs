using Alexa.DTOs;
using Microsoft.AspNetCore.Mvc;
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

namespace Alexa.Controllers
{
    [ApiController]
    [Route("api/cuentas")]
    public class CuentasController : ControllerBase
    {
        private readonly SecondaryDbContext context;
        private readonly IConfiguration configuration;

        public CuentasController(SecondaryDbContext context,
            IConfiguration configuration) 
        {
            this.context = context;
            this.configuration = configuration;
        }

        [HttpPost("Connecter")] 
        public async Task<IActionResult> Connecter(string utilisatrice, string passe)
        {
            //int Retorno = -1;
            //string Mensaje = "Ocurrio un error no controlado";
            //bool resultado = false;
            //var result = jsonRetorno(Retorno, Mensaje, resultado, (Retorno == 0 ? ("\"" + "EXITO" + "\"") : ("\"" + "NINGUNO" + "\"")));

            //var vutilisatrice = DecodeFrom64(utilisatrice);

            var usuario = await context.Usuario.FirstOrDefaultAsync(x => x.UsuarioId == utilisatrice && x.Password == passe && x.Activo==true);

            if (usuario == null)
            {                   
                return NotFound(new { Retorno = -1, Mensaje = "Credenciales de logueo incorrectas", URL = "error" });
            }
            else
            {
                var sToken = await ConstruirToken(utilisatrice);
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

        private async Task<RespuestaAutenticacion> ConstruirToken(string utilisatrice)
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

            await Task.Delay(10);

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

        //[HttpPost("Connecter", Name = "Connecter")]
        //public async Task<ActionResult<RespuestaAutenticacion>> Connecter(string utilisatrice, string passe)
        //{
        //    //var vutilisatrice = DecodeFrom64(utilisatrice);
        //    var query =  (from user in context.Set<Usuario>().Where(c => c.UsuarioId == utilisatrice && c.Password== passe).ToList()
        //                join reluserrol in context.Set<relUsuarioRol>().ToList().DefaultIfEmpty()
        //                    on user.UsuarioId equals reluserrol.UsuarioId
        //                join rol in context.Set<Rol>().ToList()
        //                    on reluserrol.RolId equals rol.RolId
        //                 select new { rol.Descripcion, rol.RolId, user.Correo, user.Nombres, user.Apellidos }).ToList();                       

        //    if (query.Count > 0)
        //    {
        //        var roledescripcion = query[0].Descripcion.ToString();
        //        var roleid = query[0].RolId.ToString();
        //        var correo = query[0].Correo.ToString();
        //        var nombres = query[0].Nombres.ToString();
        //        var apellidos = query[0].Apellidos.ToString();

        //        //var s = await ConstruirToken(utilisatrice, roledescripcion, roleid, correo, nombres, apellidos);
        //        //var t = s.Token.ToString();

        //        return  await ConstruirToken(utilisatrice, roledescripcion, roleid, correo, nombres, apellidos);

        //    }
        //    else { return BadRequest(0); }       
        //}

        //private async Task<RespuestaAutenticacion> ConstruirToken2(string utilisatrice, string role, string roleid, string correo, string nombres, string apellidos)
        //{
        //    var claims = new List<Claim>();
        //        claims = new List<Claim>()
        //        {
        //            new Claim("UTILISATRICE", utilisatrice),
        //            new Claim("ROLE", role)
        //        };

        //    var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["llavejwt"]));
        //    var creds = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

        //    var expiracion = DateTime.UtcNow.AddHours(12);

        //    var securityToken = new JwtSecurityToken(issuer: null, audience: null, claims: claims,
        //        expires: expiracion, signingCredentials: creds);

        //    return new RespuestaAutenticacion()
        //    {
        //        Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
        //        Expiracion = expiracion,
        //        RolDescripcion = role,
        //        RolId = roleid,
        //        Correo = correo,
        //        Nombres = nombres,
        //        Apellidos = apellidos
        //    };
        //}
    }
}
