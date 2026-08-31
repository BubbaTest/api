using Alexa.DAL.Seguridad;
using Alexa.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Data;
using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Alexa.Controllers
{
    [ApiController]
    [Route("endpoint/sisanom")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class SisanomController : ControllerBase
    {
        private readonly SisanomDbContext context;
        private readonly IConfiguration configuration;

        public SisanomController(SisanomDbContext context,
            IConfiguration configuration)
        {
            this.context = context;
            this.configuration = configuration;
        }

        [HttpPost("Connecter")]
        public async Task<IActionResult> Connecter(string utilisatrice, string passe)
        {

            var usuario = await context.Usuario.FirstOrDefaultAsync(x => x.UsuarioId == utilisatrice && x.Password == passe && x.Activo == true);

            if (usuario == null)
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
            //    var sToken = ConstruirToken(utilisatrice);
            //    var olista = new List<object>();
            //    DbCommand cmd;
            //    DbDataReader rdr;
            //    string sql = "EXEC sde.spUsuarioValidar @UsuarioId, @Password, @VIN";
            //    List<SqlParameter> parms = new List<SqlParameter>
            //    { 
            //        // Create parameters    
            //        new SqlParameter { ParameterName = "@UsuarioId", Value = utilisatrice },
            //        new SqlParameter { ParameterName = "@Password", Value = passe },
            //        new SqlParameter { ParameterName = "@VIN", Value = sToken.Token }
            //    };
            //    cmd = context.Database.GetDbConnection().CreateCommand();
            //    cmd.Parameters.AddRange(parms.ToArray());
            //    cmd.CommandText = sql;

            //    // Open database connection  
            //    context.Database.OpenConnection();

            //    // Create a DataReader  
            //    rdr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);

            //    while (rdr.Read())
            //    {
            //        string jsonString = "";
            //        if (rdr[0] != null)
            //        {
            //            jsonString = rdr[0].ToString() ?? "[]";
            //            dynamic catalogos = JsonConvert.DeserializeObject(jsonString);
            //            olista.Add(catalogos);
            //        };
            //    }
            //    rdr.Close();
            //    return Ok(olista);
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
