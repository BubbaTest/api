using Alexa;
using Alexa.DAL;
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
using Microsoft.EntityFrameworkCore.SqlServer;
using Newtonsoft.Json;
using System.Collections;
using System.Data.SqlClient;
using System.Text.Json.Nodes;
using System.Runtime.Intrinsics.Arm;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using static System.Collections.Specialized.BitVector32;
using Microsoft.AspNetCore.Cors;

namespace Alexa.Controllers
{
    [ApiController]
    [Route("endpoint/catalogos")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class CatalogosController : ControllerBase
    {
        private readonly CatalogsDbContext context;

        public CatalogosController(CatalogsDbContext context)
        {
            this.context = context;
        }        

        [HttpGet("Listar/{seccion}")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ListaCatalogos([FromRoute] string seccion)
        {
            var olista = new List<object>();
            DbCommand cmd;
            DbDataReader rdr;
            string sql = "EXEC dbo.Catalogos" + seccion;

            cmd = context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = sql;

            // Open database connection  
            context.Database.OpenConnection();

            // Create a DataReader  
            rdr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);

            while (rdr.Read())
            {
                string jsonString;
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
