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
using System.Text.Json;
using Alexa.DAL.Certificado;

namespace Alexa.Controllers
{
    [ApiController]
    [Route("endpoint/cenagro")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class CenagroController : ControllerBase
    {
        private readonly CenagroDbContext context;
        private readonly IConfiguration configuration;

        public CenagroController(CenagroDbContext context,
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

        [HttpPost("Certificado")]
        public async Task<ActionResult<string>> ProcesarJsonYGenerarHtml(string cedula, string tipo)
        {
            var certificado = await context.Certificados.FirstOrDefaultAsync(x => x.CedulaCorrecta == cedula && x.Tipo == tipo);
            if (certificado == null)
                return NotFound("No se encontró un certificado con la cédula proporcionada.");
            if (certificado.Activo)
                return StatusCode(StatusCodes.Status403Forbidden, "No tiene permiso para acceder a este certificado.");

            var fechaEnLetras = ObtenerFechaEnLetras(DateTime.Now);

            var htmlFormateado = $@"
            <h4>A. {certificado.NombresApellidos}</h4>
            <p>Por su invaluable labor como {certificado.Cargo} del municipio de {certificado.Municipio}, departamento de {certificado.Departamento}</p>               
            <p>en el V Censo Nacional Agropecuario (V CENAGRO) durante el período {certificado.FechaIngreso} al {certificado.FechaBaja}</p>
            <p>Dado en la ciudad de Managua a los {fechaEnLetras}</p>            
            <input type=""hidden"" id=""hiddencodigo"" value=""{certificado.N}"">";

            return Ok(htmlFormateado.Trim());
        }

        // Método auxiliar para convertir la fecha a texto en español
        private string ObtenerFechaEnLetras(DateTime fecha)
        {
            string dia = NumeroALetras(fecha.Day);
            string mes = fecha.ToString("MMMM", new System.Globalization.CultureInfo("es-ES"));
            int anio = fecha.Year;

            return $"{dia} días del mes de {mes} del {anio}";
        }

        // Método para convertir números a letras (solo para días 1-31)
        private string NumeroALetras(int numero)
        {
            string[] unidades = { "", "uno", "dos", "tres", "cuatro", "cinco", "seis", "siete", "ocho", "nueve", "diez",
                          "once", "doce", "trece", "catorce", "quince", "dieciséis", "diecisiete", "dieciocho", "diecinueve", "veinte",
                          "veintiuno", "veintidós", "veintitrés", "veinticuatro", "veinticinco", "veintiséis", "veintisiete", "veintiocho", "veintinueve", "treinta",
                          "treinta y uno" };

            if (numero >= 1 && numero <= 31)
                return unidades[numero];
            else
                return numero.ToString(); // fallback en caso de número fuera de rango
        }

        [HttpPost("Bloquear")]
        public async Task<bool> ActivarCertificadoAsync(string clave)
        {
            var certificado = await context.Certificados.FirstOrDefaultAsync(c => c.N == clave);
            if (certificado == null)
                return false; // No se encontró el certificado
            certificado.Activo = true;
            await context.SaveChangesAsync();
            return true; // Actualización exitosa
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
