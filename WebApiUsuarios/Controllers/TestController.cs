using Alexa.DAL.Capacitacion;
using Alexa.DAL.IPP;
using Alexa.DTOs;
using Alexa.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using System.Data;
using System.Linq;

namespace Alexa.Controllers
{
    [ApiController]
    [Route("endpoint/cipp")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class TestController : ControllerBase
    {
        private readonly LocalleDbContext context;
        private readonly IConfiguration configuration;
        // 1. Inyecta IDbContextFactory<Alexa.IppDbContext> en tu servicio/controlador en lugar de DbContext directamente
        private readonly IDbContextFactory<LocalleDbContext> _contextFactory;

        public TestController(LocalleDbContext context, IConfiguration configuration, IDbContextFactory<LocalleDbContext> contextFactory)
        {
            this.context = context;
            this.configuration = configuration;
            _contextFactory = contextFactory;
        }

        [NoCache]
        [HttpGet("TestList")]
        public async Task<IActionResult> TestList()
        {
            var test = await context.Test.AsNoTracking()               
                .ToListAsync();

            return Ok(test);
        }
    }
}
