using Alexa;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Alexa.DAL;
using Microsoft.EntityFrameworkCore.SqlServer;
using Newtonsoft.Json;

using System.Collections;
using System.Data.SqlClient;
using System.Text.Json.Nodes;
using System.Runtime.Intrinsics.Arm;
using Alexa.DAL.Seguridad;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Newtonsoft.Json.Linq;

namespace WebApiUsuarios.Controllers
{
    [ApiController]
    [Route("api/sp")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class StoreProController : ControllerBase
    {
        private readonly SecondaryDbContext context;
        public StoreProController(SecondaryDbContext context)
        {
            this.context = context;
        }

        [HttpGet("{Idss}/{pass}")]
        public async Task<IActionResult> GetById(string Idss, string pass)
        {
            string sql = "EXEC Student @UsuarioId, @Pass";
            List<SqlParameter> parms = new List<SqlParameter>
            {
                // Create parameter(s)    
                new SqlParameter { ParameterName = "@UsuarioId", Value = Idss },
                new SqlParameter { ParameterName = "@Pass", Value = pass }
            };
            //var Sqlstr = "EXEC Student @UsuarioId=" + Idss + ,"@Pass=" + pass;
            var studentList = await context.Usuario.FromSqlRaw(sql, parms.ToArray()).ToListAsync();
            return Ok(studentList);

        }


        [HttpGet("listadoCatalogo")]
        public async Task<IActionResult> GetCatalogos()
        {
            var olista = new List<object>();
            DbCommand cmd;
            DbDataReader rdr;
            string sql = "EXEC sde.CatalogosPorSeccion2";

            cmd = context.Database.GetDbConnection().CreateCommand();
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

        [HttpPost("{Idss}/{correo}")]
        public async Task<IActionResult> UpdateListPrice(string Idss, string correo)
        {
            string sql = "EXEC StudentUpsateCorreo @UsuarioId, @Correo, @Retorno OUTPUT, @Mensaje OUTPUT";

            List<SqlParameter> parms = new List<SqlParameter>
    { 
        // Create parameters    
        new SqlParameter { ParameterName = "@UsuarioId", Value = Idss },
        new SqlParameter { ParameterName = "@Correo", Value = correo }
    };
            SqlParameter pRetorno = new SqlParameter("@Retorno", SqlDbType.Int);
            pRetorno.Direction = ParameterDirection.Output;
            parms.Add(pRetorno);
            SqlParameter pMensaje = new SqlParameter("@Mensaje", SqlDbType.NVarChar, 1024);
            pMensaje.Direction = ParameterDirection.Output;
            parms.Add(pMensaje);

            var JsonRetorno = await context.Database.ExecuteSqlRawAsync(sql, parms.ToArray());
            var Retorno = Convert.ToInt32(pRetorno.Value);
            var Mensaje = Convert.ToString(pMensaje.Value);


            Debugger.Break();

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> MultipleResultSets()
        {
            List<Usuario> black = new List<Usuario>();
            List<Rol> red = new List<Rol>();
            DbCommand cmd;
            DbDataReader rdr;

            string sql = "EXEC StudentMultipleResult";

            // Build command object  
            cmd = context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = sql;

            // Open database connection  
            context.Database.OpenConnection();

            // Create a DataReader  
            rdr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);

            // Build collection of Black products  
            while (rdr.Read())
            {
                black.Add(new Usuario
                {
                    UsuarioId = rdr.GetString(0),  //rdr.GetInt32(0),
                    Nombres = rdr.GetString(1),
                    Apellidos = rdr.GetString(2),
                    Password = rdr.GetString(3),
                    Correo = rdr.GetString(4),
                    Activo = rdr.GetBoolean(5)
                });
            }

            // Advance to the next result set  
            rdr.NextResult();

            // Build collection of Red products  
            while (rdr.Read())
            {
                red.Add(new Rol
                {
                    RolId = rdr.GetString(0),
                    Descripcion = rdr.GetString(1),
                    Activo = rdr.GetBoolean(2)
                });
            }

            Debugger.Break();

            // Close Reader and Database Connection  
            rdr.Close();
            return Ok();
        }

        [HttpGet("PruebaCata")]
        public async Task<IActionResult> PruebaCata()
        {
            var olista = new List<object>();
            var olista1 = new List<object>();

            DbCommand cmd;
            DbDataReader rdr;

            string sql = "EXEC sde.MultipleCatalogosPorSeccion2";

            // Build command object  
            cmd = context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = sql;

            // Open database connection  
            context.Database.OpenConnection();

            // Create a DataReader  
            rdr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);

            // Build collection of Black products  
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

            // Advance to the next result set  
            rdr.NextResult();

            // Build collection of Red products  
            while (rdr.Read())
            {

                string jsonString = "";
                if (rdr[0] != null)
                {
                    jsonString = rdr[0].ToString() ?? "[]";
                    dynamic catalogos = JsonConvert.DeserializeObject(jsonString);
                    olista1.Add(catalogos);
                };

            }

            //Debugger.Break();

            // Close Reader and Database Connection  
            rdr.Close();
            //return Ok(jsonString1);
            return Ok(new { Retorno = olista, Mensaje = olista1 });
        }

        [HttpPost("Login")] ///{usuarioid}/{pass}
        public async Task<IActionResult> Login(string usuarioid, string pass)
        {
            var olista = new List<object>();
            DbCommand cmd;
            DbDataReader rdr;
            string sql = "EXEC sde.spUsuarioValidar @UsuarioId, @Password";
            List<SqlParameter> parms = new List<SqlParameter>
    { 
        // Create parameters    
        new SqlParameter { ParameterName = "@UsuarioId", Value = usuarioid },
        new SqlParameter { ParameterName = "@Password", Value = pass }
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
}
