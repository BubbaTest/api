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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Http;
using Alexa.DAL;

namespace Alexa.Controllers
{
    [ApiController]
    [Route("endpoint/capacitacion")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class CapacitacionController : ControllerBase
    {
        private readonly CapacitacionDbContext context;
        private readonly IConfiguration configuration;

        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public CapacitacionController(CapacitacionDbContext context, //UserManager<Usuario> userManager,  RoleManager<IdentityRole> roleManager
            IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            this.context = context;
            this.configuration = configuration;
            //_userManager = userManager;
            //_roleManager = roleManager;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpPost("CreateRole")]
        public async Task<IActionResult> CreateRole(CreateRoleDTO roleDTO)
        {

            var response = await _roleManager.CreateAsync(new IdentityRole
            {
                Name = roleDTO.RoleName
            });

            if (response.Succeeded)
            {
                return Ok("New Role Created");
            }
            else
            {
                return BadRequest(response.Errors);
            }
        }


        [HttpPost("AssignRoleToUser")]
        public async Task<IActionResult> AssignRoleToUser(AssignRoleToUserDTO assignRoleToUserDTO)
        {

            var userDetails = await _userManager.FindByEmailAsync(assignRoleToUserDTO.Email);

            if (userDetails != null)
            {

                var userRoleAssignResponse = await _userManager.AddToRoleAsync(userDetails, assignRoleToUserDTO.RoleName);

                if (userRoleAssignResponse.Succeeded)
                {
                    return Ok("Role Assigned to User: " + assignRoleToUserDTO.RoleName);
                }
                else
                {
                    return BadRequest(userRoleAssignResponse.Errors);
                }
            }
            else
            {
                return BadRequest("There are no user exist with this email");
            }


        }

        [HttpPost("Connecter")]
        public async Task<IActionResult> Connecter()
        {
            var usuario = await (from cust in context.Usuario
                                 join ord in context.relUsuarioRol
                                 on new { a = cust.UsuarioId } equals new { a = ord.UsuarioId } into ps
                                 from ord in ps.DefaultIfEmpty()
                                 where ord.RolId == "Supervisor"
                                 select new
                                 {
                                     cust.UsuarioId,
                                     cust.Password // Asegúrate de que este campo exista en tu modelo
                                 }).ToListAsync();


            return Ok(usuario);

        }

        [HttpGet("mapa")]
        public async Task<ActionResult<IEnumerable<DatosPuntosMapa>>> GetPuntos()
        {
            return await context.DatosPuntosMapas.ToListAsync();
        }

        [HttpPost("Validar")]
        public async Task<IActionResult> Validar(user usuarios)
        {
            var user = await context.Usuario.FirstOrDefaultAsync(c => c.Correo == usuarios.UserName && c.Password == usuarios.Password);
            if (user == null) { return Unauthorized(); }
            else
            {
                string accessToken = GenerateAccessToken(user);
                var refreshToken = GenerateRefreshToken();
                user.RefreshToken = refreshToken;
                await context.SaveChangesAsync();

                var response = new MainResponse
                {
                    Content = new AuthenticationResponse
                    {
                        RefreshToken = refreshToken,
                        AccessToken = accessToken
                    },
                    IsSuccess = true,
                    ErrorMessage = ""
                };
                return Ok(response);
            };

        }


        [AllowAnonymous]
        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest refreshTokenRequest)
        {
            var response = new MainResponse();
            if (refreshTokenRequest is null)
            {
                response.ErrorMessage = "Invalid  request";
                return BadRequest(response);
            }

            var principal = GetPrincipalFromExpiredToken(refreshTokenRequest.AccessToken);

            if (principal != null)
            {
                var email = principal.Claims.FirstOrDefault(f => f.Type == ClaimTypes.Email);

                var user = await _userManager.FindByEmailAsync(email?.Value);

                if (user is null || user.RefreshToken != refreshTokenRequest.RefreshToken)
                {
                    response.ErrorMessage = "Invalid Request";
                    return BadRequest(response);
                }

                string newAccessToken = GenerateAccessToken(user);
                string refreshToken = GenerateRefreshToken();

                user.RefreshToken = refreshToken;
                await _userManager.UpdateAsync(user);

                response.IsSuccess = true;
                response.Content = new AuthenticationResponse
                {
                    RefreshToken = refreshToken,
                    AccessToken = newAccessToken
                };
                return Ok(response);
            }
            else
            {
                return ErrorResponse.ReturnErrorResponse("Invalid Token Found");
            }
        }

        private string GenerateAccessToken(Usuario user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var keyDetail = Encoding.UTF8.GetBytes(configuration["JWT:Key"]);

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UsuarioId),
                    new Claim(ClaimTypes.Name, $"{user.Nombres} { user.Apellidos}"),
                    new Claim(ClaimTypes.Email, user.Correo),

            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Audience = configuration["JWT:Audience"],
                Issuer = configuration["JWT:Issuer"],
                Expires = DateTime.UtcNow.AddMinutes(30),
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyDetail), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var keyDetail = Encoding.UTF8.GetBytes(configuration["JWT:Key"]);
            var tokenValidationParameter = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["JWT:Issuer"],
                ValidAudience = configuration["JWT:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(keyDetail),
            };

            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameter, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");
            return principal;
        }

        private string GenerateRefreshToken()
        {

            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        [AllowAnonymous]
        [HttpPost("RegisterUser")]
        public async Task<IActionResult> RegisterUser(Usuario usuarioDto)
        {
            // Validaciones de entrada
            if (string.IsNullOrWhiteSpace(usuarioDto.UsuarioId))
            {
                return BadRequest("El campo Nombres es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(usuarioDto.Nombres))
            {
                return BadRequest("El campo Nombres es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(usuarioDto.Apellidos))
            {
                return BadRequest("El campo Apellidos es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(usuarioDto.Password) || usuarioDto.Password.Length < 6)
            {
                return BadRequest("El campo Password es obligatorio y debe tener al menos 6 caracteres.");
            }

            if (string.IsNullOrWhiteSpace(usuarioDto.Correo) || !IsValidEmail(usuarioDto.Correo))
            {
                return BadRequest("El campo Correo es obligatorio y debe tener un formato válido.");
            }

            // Validar si el UsuarioId ya existe
            var existingUser = await context.Usuario
                .FirstOrDefaultAsync(u => u.UsuarioId == usuarioDto.UsuarioId);

            if (existingUser != null)
            {
                return BadRequest("El UsuarioId ya existe.");
            }

            var userToBeCreated = new Usuario
            {
                UsuarioId = usuarioDto.UsuarioId,
                Nombres = usuarioDto.Nombres,
                Apellidos = usuarioDto.Apellidos,
                Password = usuarioDto.Password,
                Correo = usuarioDto.Correo,
                Activo = true
            };

            try
            {
                await context.Usuario.AddAsync(userToBeCreated); // Asegúrate de que 'Usuarios' sea el DbSet correcto

                // Guardar los cambios en la base de datos
                var result = await context.SaveChangesAsync();

                // Verificar si la creación fue exitosa
                if (result > 0) // Si se han guardado cambios, el resultado será mayor que 0
                {
                    return Ok("User  Created");
                }
                else
                {
                    return BadRequest("User  creation failed");
                }
            }
            catch (Exception ex)
            {
                // Manejo de excepciones
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }

            //var response = await context.CreateAsync(userToBeCreated);
            //if (response.Succeeded)
            //{
            //    return Ok("User Created");
            //}
            //else
            //{
            //    return BadRequest(response.Errors);
            //}
        }

        // Método para validar el formato del correo electrónico
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

    }
}
