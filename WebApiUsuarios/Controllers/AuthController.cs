using Alexa.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
    [Route("api/[controller]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AuthController : ControllerBase
    {
        private readonly EinkommenDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(EinkommenDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("Connecter")]
        public async Task<IActionResult> Connecter(string utilisatrice, string passe, string sistema)
        {
            // 1. Generar sesión aleatoria
            var bytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            var session = BitConverter.ToString(bytes).Replace("-", "");
            var ipAddressString = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

            // 2. Evaluar si el usuario existe, está activo, contraseña coincide y tiene rol asignado
            // Se usa FirstOrDefaultAsync para obtener el usuario y validar en una sola consulta eficiente
            var usuarioValido = await (from u in _context.Usuario
                                       join r in _context.relUsuarioRol on u.UsuarioId equals r.UsuarioId
                                       where u.UsuarioId == utilisatrice && u.Password == passe && u.Activo == true
                                       select u).AnyAsync();

            if (!usuarioValido)
            {
                return NotFound(new
                {
                    Retorno = -1,
                    Mensaje = "Credenciales de logueo incorrectas / Usuario Inactivo o sin Rol",
                    URL = "Usuario/Login"
                });
            }

            // 3. Generar token
            var sToken = ConstruirToken(utilisatrice);

            // 4. Ejecutar procedimiento almacenado sde.spUsuarioValidar
            var olista = new List<object>();

            using (var cmd = _context.Database.GetDbConnection().CreateCommand())
            {
                cmd.CommandText = "EXEC sde.spUsuarioValidar @UsuarioId, @Password, @sistema, @session, @ip, @VIN";
                cmd.Parameters.Add(new SqlParameter("@UsuarioId", utilisatrice));
                cmd.Parameters.Add(new SqlParameter("@Password", passe));
                cmd.Parameters.Add(new SqlParameter("@sistema", sistema));
                cmd.Parameters.Add(new SqlParameter("@session", session));
                cmd.Parameters.Add(new SqlParameter("@ip", ipAddressString));
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

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expirationHours = _configuration.GetValue<int>("JWT:ExpiresInHours", 12);
            var expiracion = DateTime.UtcNow.AddHours(expirationHours);

            var securityToken = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
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
            var claims = new List<Claim>
            {
                new Claim("UTILISATRICE", utilisatrice)
            };

            var llaveJwt = _configuration["llavejwt"];
            if (string.IsNullOrEmpty(llaveJwt))
            {
                throw new InvalidOperationException("La configuración 'llavejwt' no está disponible.");
            }

            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(llaveJwt));
            var creds = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

            var expiracion = DateTime.UtcNow.AddHours(12);

            var securityToken = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: expiracion,
                signingCredentials: creds);

            return new RespuestaAutenticacion
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                Expiracion = expiracion
            };
        }
    }
}
