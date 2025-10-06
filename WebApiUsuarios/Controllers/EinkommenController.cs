using Alexa.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using Newtonsoft.Json;
using Alexa.DAL.Seguridad;
using System.Security.Cryptography;

namespace Alexa.Controllers
{
    [ApiController]
    [Route("endpoint/einkommen")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class EinkommenController : ControllerBase
    {
        private readonly EinkommenDbContext context;
        private readonly IConfiguration configuration;

        public EinkommenController(EinkommenDbContext context,
            IConfiguration configuration)
        {
            this.context = context;
            this.configuration = configuration;
        }

        [HttpPost("Connecter")]
        public async Task<IActionResult> Connecter(string utilisatrice, string passe, string sistema)
        {
            var bytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            var session = BitConverter.ToString(bytes).Replace("-", "");
            var ipAddressString = HttpContext.Connection.RemoteIpAddress.ToString();

            var usuario = await (from cust in context.Usuario
                                 join ord in context.relUsuarioRol
                                 on new { a = cust.UsuarioId } equals new { a = ord.UsuarioId } into ps
                                 from ord in ps.DefaultIfEmpty()
                                 where cust.UsuarioId == utilisatrice && cust.Password == passe && cust.Activo == true
                                 select cust).ToListAsync();

            if (usuario.Count == 0)
            {
                return NotFound(new { Retorno = -1, Mensaje = "Credenciales de logueo incorrectas / Usuario Inactivo o sin Rol", URL = "Usuario/Login" });
            }
            else
            {
                var sToken = ConstruirToken(utilisatrice);
                var olista = new List<object>();
                DbCommand cmd;
                DbDataReader rdr;
                string sql = "EXEC sde.spUsuarioValidar @UsuarioId, @Password, @sistema, @session, @ip, @VIN";
                List<SqlParameter> parms = new List<SqlParameter>
                { 
                    // Create parameters    
                    new SqlParameter { ParameterName = "@UsuarioId", Value = utilisatrice },
                    new SqlParameter { ParameterName = "@Password", Value = passe },
                    new SqlParameter { ParameterName = "@sistema", Value = sistema },
                    new SqlParameter { ParameterName = "@session", Value = session },
                    new SqlParameter { ParameterName = "@ip", Value = ipAddressString },
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
                    }
                    ;
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

        //public static class SessionManager
        //{
        //    private static List<SessionLog> _sessions = new List<SessionLog>();

        //    public static void RegisterLogin(SessionLog user)
        //    {
        //        if (user != null)
        //        {
        //            _sessions.RemoveAll(u => u.UsuarioId == user.UsuarioId);
        //            _sessions.Add(user);
        //        }
        //    }

        //    public static void DeregisterLogin(SessionLog user)
        //    {
        //        if (user != null)
        //            _sessions.RemoveAll(u => u.UsuarioId == user.UsuarioId && u.Sesion == user.Sesion);
        //    }

        //    public static bool ValidateCurrentLogin(SessionLog user)
        //    {
        //        return user != null && _sessions.Any(u => u.UsuarioId == user.UsuarioId && u.Sesion == user.Sesion);
        //    }
        //}

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
