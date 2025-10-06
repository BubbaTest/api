using Alexa.DAL.IPP;
using Alexa.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Data;
using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Alexa.Controllers
{
    [ApiController]
    [Route("endpoint/cipp")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class CippController : ControllerBase
    {
        private readonly IppDbContext context;
        private readonly IConfiguration configuration;

        public CippController(IppDbContext context, IConfiguration configuration)
        {
            this.context = context;
            this.configuration = configuration;
        }

        private static bool IsValidRequestDay(int day)
        {
            return day == 1;
        }

        private static (int day, int month, int year) GetCurrentDate()
        {
            DateTime now = DateTime.Now;
            return (now.Day, now.Month, now.Year);
        }

        [HttpGet("Muestra/{empleado}")]
        public async Task<IActionResult> MuestrasFiltradas([FromRoute] string empleado)
        {
            try
            {
                // Validar que el parámetro "empleado" no esté vacío
                if (string.IsNullOrEmpty(empleado))
                {
                    return BadRequest("El parámetro 'empleado' es requerido.");
                }

                // Construir la consulta LINQ
                var muestra = await (from ei in context.Muestra
                                     join m in context.AsignacionPersonal on ei.ObjIdEstablecimientoCanasta equals m.ObjIdEstablecimientoCanasta
                                     join l in context.SEC_EMPLEADO on m.ObjIdCatPersonal equals l.ID_EMPLEADO
                                     join p in context.EstablecimientoCanasta on ei.ObjIdEstablecimientoCanasta equals p.IdEstablecimientoCanasta
                                     join e in context.CatEstablecimiento on p.ObjIdCatEstablecimiento equals e.IdCatEstablecimiento
                                     join v in context.CatVariedad on ei.ObjIdCatVariedad equals v.IdCatVariedad
                                     join c in context.CatCanasta on p.ObjIdCatCanasta equals c.IdCatCanasta
                                     where ei.Activo && l.Activo && m.Activo && l.USERLOGIN == empleado && l.Activo && e.Activo && c.ObjIdCatEncuesta == 2 && c.Activo
                                     select new
                                     {
                                         ei.IdMuestra,
                                         ei.ObjIdEstablecimientoCanasta,
                                         ei.ObjIdCatVariedad,
                                         ei.ObjIdDia,
                                         ei.Detalle,
                                         ei.NVeces,
                                         NombreEstablecimiento = e.Nombre,
                                         v.IdCatVariedad,
                                         NombreVariedad = v.Descripcion,
                                         p.ObjIdCatCanasta,
                                         NombreCanasta = c != null ? c.Nombre : (string)null
                                     }).ToListAsync(); // Materializar la consulta                

                var establecimiento = await (from ei in context.Muestra
                                             join m in context.AsignacionPersonal on ei.ObjIdEstablecimientoCanasta equals m.ObjIdEstablecimientoCanasta
                                             join l in context.SEC_EMPLEADO on m.ObjIdCatPersonal equals l.ID_EMPLEADO
                                             join p in context.EstablecimientoCanasta on ei.ObjIdEstablecimientoCanasta equals p.IdEstablecimientoCanasta
                                             join e in context.CatEstablecimiento on p.ObjIdCatEstablecimiento equals e.IdCatEstablecimiento
                                             join c in context.CatCanasta on p.ObjIdCatCanasta equals c.IdCatCanasta
                                             where ei.Activo && m.Activo && l.Activo && l.USERLOGIN == empleado && e.Activo && c.Activo && e.Activo && c.ObjIdCatEncuesta == 2
                                             select new
                                             {
                                                 c.IdCatCanasta,
                                                 p.IdEstablecimientoCanasta,
                                                 e.IdCatEstablecimiento,
                                                 e.ObjCodMuni,
                                                 e.Razon_soc,
                                                 NombreEstablecimiento = e.Nombre,
                                                 e.Encargado,
                                                 e.Cargo,
                                                 e.Telefono,
                                                 e.Direccion,
                                                 e.DiaHabil,
                                                 FechaDefinidaRecoleccion = (from ec in context.EstablecimientoCanasta
                                                                             join ce in context.CatEstablecimiento on ec.ObjIdCatEstablecimiento equals ce.IdCatEstablecimiento
                                                                             join cal in context.CatCalendario on ce.DiaHabil equals Convert.ToInt32(cal.DiaLaboral)
                                                                             where ec.IdEstablecimientoCanasta == p.IdEstablecimientoCanasta && ce.Activo
                                                                             && cal.Fecha.Year == DateTime.Now.Year
                                                                             && cal.Fecha.Month == DateTime.Now.Month
                                                                             select cal.Fecha).FirstOrDefault()
                                             }).Distinct().ToListAsync(); // Materializar la consulta

                // Eliminar duplicados si es necesario (opcional)
                //var establecimientosUnicos = establecimiento
                //    .GroupBy(x => x.IdEstablecimientoCanasta)
                //    .Select(g => g.First())
                //    .ToList();


                //var variedad = await (from ei in context.Muestra
                //                      join m in context.AsignacionPersonal on ei.ObjIdEstablecimientoCanasta equals m.ObjIdEstablecimientoCanasta
                //                      join l in context.SEC_EMPLEADO on m.ObjIdCatPersonal equals l.ID_EMPLEADO
                //                      join p in context.EstablecimientoCanasta on ei.ObjIdEstablecimientoCanasta equals p.IdEstablecimientoCanasta
                //                      join e in context.CatEstablecimiento on p.ObjIdCatEstablecimiento equals e.IdCatEstablecimiento
                //                      join v in context.CatVariedad on ei.ObjIdCatVariedad equals v.IdCatVariedad
                //                      from c in context.CatCanasta
                //                          .Where(x => x.IdCatCanasta == p.ObjIdCatCanasta)
                //                          .DefaultIfEmpty() // LEFT JOIN
                //                      where ei.Activo && l.Activo && c.Activo && l.ID_EMPLEADO == empleado && c.ObjIdCatEncuesta == 2
                //                      select new
                //                      {
                //                          v.IdCatVariedad,
                //                          ei.ObjIdEstablecimientoCanasta,
                //                          v.ObjIdCatCanasta,
                //                          NombreVariedad = v.Descripcion
                //                      }).ToListAsync(); // Materializar la consulta

                // Devolver la respuesta con ambos objetos los resultados
                return Ok(new
                {
                    Muestra = muestra,
                    Establecimiento = establecimiento
                });
            }
            catch (Exception ex)
            {
                // Manejar errores y devolver una respuesta de error
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("Validamuestra/{empleado}/{canasta}/{municipio}")]
        public async Task<IActionResult> Validamuestra([FromRoute] string empleado, [FromRoute] int canasta, [FromRoute] int municipio)
        {
            var querys = await (from ei in context.Muestra
                                join m in context.AsignacionPersonal on ei.ObjIdEstablecimientoCanasta equals m.ObjIdEstablecimientoCanasta
                                join l in context.SEC_EMPLEADO on m.ObjIdCatPersonal equals l.ID_EMPLEADO
                                join p in context.EstablecimientoCanasta on ei.ObjIdEstablecimientoCanasta equals p.IdEstablecimientoCanasta
                                join e in context.CatEstablecimiento on p.ObjIdCatEstablecimiento equals e.IdCatEstablecimiento
                                join v in context.CatVariedad on ei.ObjIdCatVariedad equals v.IdCatVariedad
                                join c in context.CatCanasta on p.ObjIdCatCanasta equals c.IdCatCanasta
                                where ei.Activo && m.Activo && l.Activo && l.USERLOGIN == empleado && p.ObjIdCatCanasta == canasta
                                    && e.Activo && e.ObjCodMuni == municipio
                                    && c.ObjIdCatEncuesta == 2 && c.Activo
                                    && !context.Detalle.Any(sp =>
                                        sp.ObjIdCatCanasta == canasta && sp.ObjCodMuni == municipio &&
                                        sp.ObjIdEstablecimientoCanasta == ei.ObjIdEstablecimientoCanasta &&
                                        sp.ObjIdCatVariedad == ei.ObjIdCatVariedad &&
                                        sp.FechaDefinidaRecoleccion.Year == DateTime.Now.Year &&
                                        sp.FechaDefinidaRecoleccion.Month == DateTime.Now.Month)
                                select new
                                {
                                    p.ObjIdCatCanasta,
                                    e.ObjCodMuni,
                                    ei.ObjIdEstablecimientoCanasta,
                                    NombreEstablecimiento = e.Nombre,
                                    NombreVariedad = v.Descripcion,
                                    NombreCanasta = c != null ? c.Nombre : (string)null
                                }).ToListAsync(); // Materializar la consulta                  

            // Establecer encabezados para evitar caché
            Response.Headers["Cache-Control"] = "no-store";
            Response.Headers["Pragma"] = "no-cache";

            if (querys.Count > 0)
            {
                return Ok(querys);
            }

            //return Ok(new { mensaje = "No tiene variedades pendientes." });
            return Ok(querys);
        }

        [HttpGet("Catalogos/{empleado}")]
        public async Task<IActionResult> CatalogosListados([FromRoute] string empleado)
        {
            var calendario = await context.CatCalendario
               .Where(c => c.Fecha.Year == DateTime.Now.Year)
                .ToListAsync();

            var municipio = await (from ei in context.Muestra
                                   join m in context.AsignacionPersonal on ei.ObjIdEstablecimientoCanasta equals m.ObjIdEstablecimientoCanasta
                                   join l in context.SEC_EMPLEADO on m.ObjIdCatPersonal equals l.ID_EMPLEADO
                                   join t in context.EstablecimientoCanasta on ei.ObjIdEstablecimientoCanasta equals t.IdEstablecimientoCanasta
                                   join n in context.CatEstablecimiento on t.ObjIdCatEstablecimiento equals n.IdCatEstablecimiento
                                   join p in context.CatCanasta on t.ObjIdCatCanasta equals p.IdCatCanasta
                                   join z in context.SEC_MUNI on n.ObjCodMuni equals z.ID_Muni
                                   where ei.Activo && m.Activo && l.Activo && l.USERLOGIN == empleado
                                          && n.Activo && p.Activo && p.ObjIdCatEncuesta == 2
                                   group z by new { z.ID_Muni, z.NOM_MUNI, t.ObjIdCatCanasta } into g
                                   select new SEC_MUNIDTO
                                   {
                                       ID_Muni = g.Key.ID_Muni,
                                       NOM_MUNI = g.Key.NOM_MUNI,
                                       ObjIdCatCanasta = g.Key.ObjIdCatCanasta
                                   }).OrderBy(c => c.ObjIdCatCanasta).ToListAsync();


            //var municipio = await (from ei in context.Muestra
            //                       join m in context.AsignacionPersonal on ei.ObjIdEstablecimientoCanasta equals m.ObjIdEstablecimientoCanasta
            //                       join l in context.SEC_EMPLEADO on m.ObjIdCatPersonal equals l.ID_EMPLEADO
            //                       join t in context.EstablecimientoCanasta on ei.ObjIdEstablecimientoCanasta equals t.IdEstablecimientoCanasta
            //                       join p in context.CatCanasta on t.ObjIdCatCanasta equals p.IdCatCanasta
            //                       join a in context.AsignarZona on l.ID_EMPLEADO equals a.ObjIdCatPersonal
            //                       join z in context.SEC_MUNI on Convert.ToInt64(a.ObjIdMuni) equals z.ID_Muni
            //                       where ei.Activo && m.Activo && l.Activo && p.Activo && a.Activo
            //                             && l.USERLOGIN == empleado && p.ObjIdCatEncuesta == 2
            //                       group z by new { z.ID_Muni, z.NOM_MUNI, t.ObjIdCatCanasta } into g
            //                       select new SEC_MUNIDTO
            //                       {
            //                           ID_Muni = g.Key.ID_Muni,
            //                           NOM_MUNI = g.Key.NOM_MUNI,
            //                           ObjIdCatCanasta = g.Key.ObjIdCatCanasta
            //                       }).OrderBy(c => c.ID_Muni).ToListAsync();


            var canasta = await context.CatCanasta
                .Where(c => c.ObjIdCatEncuesta == 2 && c.Activo)
                .ToListAsync();

            var causal = await context.CatValorCatalogo
                .Where(v => new[] { 10, 11, 12, 50, 58 }.Contains(v.IdCatValorCatalogo))
                .ToListAsync();

            var estados = await context.CatValorCatalogo
               .Where(v => new[] { 13, 14, 15, 16 }.Contains(v.IdCatValorCatalogo))
               .ToListAsync();

            var monedas = await context.CatValorCatalogo
               .Where(v => new[] { 42, 43 }.Contains(v.IdCatValorCatalogo))
               .ToListAsync();

            var tipocambios = await context.CatTipoCambio
               .Where(c => c.Fecha.Year == DateTime.Now.Year && c.Fecha.Month == DateTime.Now.Month)
                .ToListAsync();

            var unidadmedida = await (from um in context.CatUMedVar
                                      join cv in context.CatVariedad on um.ObjIdCatVariedad equals cv.IdCatVariedad
                                      join uni in context.CatUnidadMedida on um.ObjURecolId equals uni.IdCatUnidadMedida into uniGroup
                                      from uni in uniGroup.DefaultIfEmpty()
                                      join ei in context.Muestra on um.ObjIdCatVariedad equals ei.ObjIdCatVariedad
                                      join m in context.AsignacionPersonal on ei.ObjIdEstablecimientoCanasta equals m.ObjIdEstablecimientoCanasta
                                      join l in context.SEC_EMPLEADO on m.ObjIdCatPersonal equals l.ID_EMPLEADO
                                      join p in context.EstablecimientoCanasta on ei.ObjIdEstablecimientoCanasta equals p.IdEstablecimientoCanasta
                                      join c in context.CatCanasta on p.ObjIdCatCanasta equals c.IdCatCanasta
                                      where ei.Activo && m.Activo && l.Activo && l.USERLOGIN == empleado
                                            && c.Activo && c.ObjIdCatEncuesta == 2
                                      select new
                                      {
                                          ObjIdCatVariedad = um.ObjIdCatVariedad,
                                          ObjURecolId = um.ObjURecolId,
                                          Nombre = uni.Nombre
                                      }).Distinct().ToListAsync();

            var response = new CatalogosIpp
            {
                Calendarios = calendario,
                Muncipios = municipio,
                Canasta = canasta,
                Causales = causal,
                Estados = estados,
                Monedas = monedas,
                TipoCambios = tipocambios,
                UnidadMedida = unidadmedida.Select(um => new UnidadMedidaDTO
                {
                    ObjIdCatVariedad = um.ObjIdCatVariedad,
                    ObjURecolId = um.ObjURecolId,
                    NombreUnidad = um.Nombre
                }).ToList()
            };

            return Ok(response);
        }

        [HttpGet("Previo1/{empleado}")]
        public async Task<IActionResult> DatosReferencia1([FromRoute] string empleado)
        {
            try
            {
                // Usamos la fecha de hoy
                var hoy = DateTime.Today;
                var primerDiaMesActual = new DateTime(hoy.Year, hoy.Month, 1);
                var fechaInicioMesAnterior = primerDiaMesActual.AddMonths(-1);
                var fechaFinMesAnterior = primerDiaMesActual.AddDays(-1);

                var codigosExcluidos = new[] { "CD", "CT", "RCH", "OC" };

                // Obtener IDs de estados válidos
                var estadosValidos = await (
                    from c in context.CatCatalogo
                    join v in context.CatValorCatalogo on c.IdCatCatalogo equals v.ObjIdCatCatalogo
                    where c.Codigo == "ESTADOGRABACION"
                          && c.Activo
                          && v.Activo
                          && !codigosExcluidos.Contains(v.Codigo)
                    select v.IdCatValorCatalogo
                ).ToListAsync();

                // Convertir a HashSet para mejor rendimiento en Contains
                var estadosValidosSet = new HashSet<int>(estadosValidos);

                // Subconsulta: grupo con MAX(FechaDefinidaRecoleccion)
                var maxFechasQuery = from d in context.Detalle
                                     join m in context.AsignacionPersonal on d.ObjIdEstablecimientoCanasta equals m.ObjIdEstablecimientoCanasta
                                     join l in context.SEC_EMPLEADO on m.ObjIdCatPersonal equals l.ID_EMPLEADO
                                     where d.FechaImputado == null
                                           && d.FechaDefinidaRecoleccion >= fechaInicioMesAnterior
                                           && d.FechaDefinidaRecoleccion <= fechaFinMesAnterior
                                           //&& d.ObjIdEstablecimientoCanasta == 512
                                           //&& d.ObjIdCatVariedad == 680
                                           && estadosValidosSet.Contains(d.ObjIdEstadoVar)
                                           && l.USERLOGIN == empleado
                                     group d by new
                                     {
                                         d.ObjIdEstablecimientoCanasta,
                                         d.ObjIdCatVariedad,
                                         d.ObjIdEstadoVar
                                     } into g
                                     select new
                                     {
                                         g.Key.ObjIdEstablecimientoCanasta,
                                         g.Key.ObjIdCatVariedad,
                                         g.Key.ObjIdEstadoVar,
                                         FechaMax = g.Max(x => x.FechaDefinidaRecoleccion)
                                     };

                // Consulta principal: une con las tablas y proyecta el resultado
                var query = from s in maxFechasQuery
                            join d in context.Detalle on new
                            {
                                s.ObjIdEstablecimientoCanasta,
                                s.ObjIdCatVariedad,
                                s.ObjIdEstadoVar,
                                FechaDefinidaRecoleccion = s.FechaMax
                            } equals new
                            {
                                d.ObjIdEstablecimientoCanasta,
                                d.ObjIdCatVariedad,
                                d.ObjIdEstadoVar,
                                d.FechaDefinidaRecoleccion
                            }
                            join m in context.Muestra on new
                            {
                                d.ObjIdEstablecimientoCanasta,
                                d.ObjIdCatVariedad
                            } equals new
                            {
                                m.ObjIdEstablecimientoCanasta,
                                m.ObjIdCatVariedad
                            }
                            join a in context.AsignacionPersonal on d.ObjIdEstablecimientoCanasta equals a.ObjIdEstablecimientoCanasta
                            join z in context.SEC_EMPLEADO on a.ObjIdCatPersonal equals z.ID_EMPLEADO
                            join cv in context.CatValorCatalogo on d.ObjIdEstadoVar equals cv.IdCatValorCatalogo
                            join v in context.CatVariedad on d.ObjIdCatVariedad equals v.IdCatVariedad
                            where m.Activo && z.USERLOGIN == empleado
                            select new
                            {
                                d.ObjIdEstablecimientoCanasta,
                                d.ObjIdCatVariedad,
                                d.ObjIdEstadoVar,
                                d.FechaDefinidaRecoleccion,
                                d.PrecioRealRecolectado,
                                d.PrecioCalculado,
                                Especificacion = m.Detalle,
                                m.NVeces,
                                NombreEstado = cv.Nombre,
                                d.ObjIdUnidRecolectada,
                                d.TasaCambio
                            };

                // ✅ Ahora .ToListAsync() funciona porque 'query' es IQueryable
                var resultado = await query.ToListAsync();

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("Previo/{empleado}")]
        public async Task<IActionResult> DatosReferencia([FromRoute] string empleado)
        {
            try
            {
                var hoy = DateTime.Today;

                var codigosExcluidos = new[] { "CD", "CT", "RCH", "OC" };

                // Obtener IDs de estados válidos
                var estadosValidos = await (
                    from c in context.CatCatalogo
                    join v in context.CatValorCatalogo on c.IdCatCatalogo equals v.ObjIdCatCatalogo
                    where c.Codigo == "ESTADOGRABACION"
                          && c.Activo
                          && v.Activo
                          && !codigosExcluidos.Contains(v.Codigo)
                    select v.IdCatValorCatalogo
                ).ToListAsync();

                // Convertir a HashSet para mejor rendimiento en Contains
                var estadosValidosSet = new HashSet<int>(estadosValidos);

                // Subconsulta: grupo con MAX(FechaDefinidaRecoleccion) donde PrecioRealRecolectado tiene valor y != 0
                // Busca en todo el historial pasado (sin límite al mes anterior), priorizando la última recolección válida
                var maxFechasQuery = from d in context.Detalle
                                     join m in context.AsignacionPersonal on d.ObjIdEstablecimientoCanasta equals m.ObjIdEstablecimientoCanasta
                                     join l in context.SEC_EMPLEADO on m.ObjIdCatPersonal equals l.ID_EMPLEADO
                                     where d.FechaImputado == null
                                           && d.PrecioRealRecolectado.HasValue  // Nueva condición: debe tener valor (no null)
                                           && d.PrecioRealRecolectado != 0      // Y diferente de cero
                                           && d.FechaDefinidaRecoleccion < hoy  // Limitar a fechas pasadas para evitar futuro
                                                                                //&& d.ObjIdEstablecimientoCanasta == 512  // Comentado: mantener generalidad
                                                                                //&& d.ObjIdCatVariedad == 680
                                           && estadosValidosSet.Contains(d.ObjIdEstadoVar)
                                           && l.USERLOGIN == empleado
                                     group d by new
                                     {
                                         d.ObjIdEstablecimientoCanasta,
                                         d.ObjIdCatVariedad,
                                         d.ObjIdEstadoVar
                                     } into g
                                     select new
                                     {
                                         g.Key.ObjIdEstablecimientoCanasta,
                                         g.Key.ObjIdCatVariedad,
                                         g.Key.ObjIdEstadoVar,
                                         FechaMax = g.Max(x => x.FechaDefinidaRecoleccion)
                                     };

                // Consulta principal: une con las tablas y proyecta el resultado
                // Mantiene la proyección original, incluyendo PrecioRealRecolectado para consistencia
                var query = from s in maxFechasQuery
                            join d in context.Detalle on new
                            {
                                s.ObjIdEstablecimientoCanasta,
                                s.ObjIdCatVariedad,
                                s.ObjIdEstadoVar,
                                FechaDefinidaRecoleccion = s.FechaMax
                            } equals new
                            {
                                d.ObjIdEstablecimientoCanasta,
                                d.ObjIdCatVariedad,
                                d.ObjIdEstadoVar,
                                d.FechaDefinidaRecoleccion
                            }
                            join m in context.Muestra on new
                            {
                                d.ObjIdEstablecimientoCanasta,
                                d.ObjIdCatVariedad
                            } equals new
                            {
                                m.ObjIdEstablecimientoCanasta,
                                m.ObjIdCatVariedad
                            }
                            join a in context.AsignacionPersonal on d.ObjIdEstablecimientoCanasta equals a.ObjIdEstablecimientoCanasta
                            join z in context.SEC_EMPLEADO on a.ObjIdCatPersonal equals z.ID_EMPLEADO
                            join cv in context.CatValorCatalogo on d.ObjIdEstadoVar equals cv.IdCatValorCatalogo
                            join v in context.CatVariedad on d.ObjIdCatVariedad equals v.IdCatVariedad
                            where m.Activo && z.USERLOGIN == empleado
                            select new
                            {
                                d.ObjIdEstablecimientoCanasta,
                                d.ObjIdCatVariedad,
                                d.ObjIdEstadoVar,
                                d.FechaDefinidaRecoleccion,
                                d.PrecioRealRecolectado,
                                d.PrecioCalculado,
                                Especificacion = m.Detalle,
                                m.NVeces,
                                NombreEstado = cv.Nombre,
                                d.ObjIdUnidRecolectada,
                                d.TasaCambio
                            };

                // ✅ Ahora .ToListAsync() funciona porque 'query' es IQueryable
                var resultado = await query.ToListAsync();

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost("Connecter/{empleado}")]
        public async Task<IActionResult> Connecter([FromRoute] string empleado)
        {
            // Validar que el parámetro "empleado" no esté vacío
            if (string.IsNullOrEmpty(empleado))
            {
                return BadRequest("El parámetro 'empleado' es obligatorio.");
            }

            // Obtener el día, mes y año actuales
            var (currentDay, currentMonth, currentYear) = GetCurrentDate();

            //Validar el día de la solicitud
            if (!IsValidRequestDay(currentDay))
            {
                return Unauthorized(new { mensaje = "Acceso denegado. No se puede obtener acceso fuera del período establecido." });
            }

            var query = await (from ei in context.Muestra
                               join m in context.AsignacionPersonal on ei.ObjIdEstablecimientoCanasta equals m.ObjIdEstablecimientoCanasta
                               join l in context.SEC_EMPLEADO on m.ObjIdCatPersonal equals l.ID_EMPLEADO
                               where ei.Activo && m.Activo && l.Activo && l.USERLOGIN == empleado
                               select new
                               {
                                   Usuario = l.USERLOGIN,
                                   Pass = l.PASSWD
                               }).FirstOrDefaultAsync();

            if (query == null)
            {
                return NotFound(new { mensaje = "Acceso denegado. No es un usuario activo o no tiene muestra asignada en el período." });
            }

            return Ok(query);
        }

        [HttpPost("Einkommen/{empleado}/{usuario}")]
        public async Task<IActionResult> Einkommen([FromRoute] string empleado, [FromRoute] string usuario, [FromQuery] string clave)
        {
            // Validar las credenciales no esten vacío
            if (string.IsNullOrEmpty(empleado) || string.IsNullOrEmpty(clave))
            {
                return BadRequest("Las credenciales son obligatorias.");
            }

            var query = await (from ei in context.Usuario
                               where ei.Activo && ei.UsuarioId == usuario && ei.Password == clave
                               select new
                               {
                                   ei.UsuarioId
                               }).FirstOrDefaultAsync();

            if (query == null)
            {
                return NotFound(new { mensaje = "Acceso denegado. Credenciales Invalidas." });
            }

            var result = await (from ei in context.Usuario
                                where ei.Activo && ei.UsuarioId == empleado
                                select new
                                {
                                    Usuario = ei.UsuarioId,
                                    Pass = ei.Password
                                }).FirstOrDefaultAsync();

            if (result == null)
            {
                return NotFound(new { mensaje = "Acceso denegado. No es un usuario activo o no tiene muestra asignada en el período." });
            }

            // Establecer encabezados para evitar caché
            Response.Headers["Cache-Control"] = "no-store";
            Response.Headers["Pragma"] = "no-cache";

            return Ok(result);
        }

        [HttpPost("bulksupin")]
        public async Task<ActionResult> PostupinDetalle(BulkInsDetUpEstDto bulkInsertDto)
        {
            if (bulkInsertDto == null ||
                IsNullOrEmpty(bulkInsertDto.Detalle) &&
                IsNullOrEmpty(bulkInsertDto.CatEstablecimiento))
            {
                return BadRequest("Las listas de Detalle y Establecimiento no pueden estar vacías.");
            }

            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                //1.Insertar Detalle              

                if (!IsNullOrEmpty(bulkInsertDto.Detalle))
                {
                    foreach (var item in bulkInsertDto.Detalle)
                    {
                        var existing = await context.Detalle
                            .FindAsync(item.ObjIdEstablecimientoCanasta, item.ObjIdCatVariedad, item.FechaDefinidaRecoleccion);

                        if (existing != null)
                        {
                            existing.PrecioCalculado = item.PrecioCalculado;
                            existing.PrecioRealRecolectado = item.PrecioRealRecolectado;
                            existing.Cantidad = item.Cantidad;
                            existing.FechaRecoleccion = item.FechaRecoleccion;
                            existing.TasaCambio = item.TasaCambio;
                            existing.ObjIdTipoMoneda = item.ObjIdTipoMoneda;
                            existing.ObjIdEstadoVar = item.ObjIdEstadoVar;
                            existing.ObjIdUnidRecolectada = item.ObjIdUnidRecolectada;
                            existing.Observacion = item.Observacion;
                            existing.UsuarioCreacion = item.UsuarioCreacion;
                        }
                        else
                        {
                            await context.AddAsync(item);
                        }
                    }

                    await context.SaveChangesAsync();
                }

                // 2. Actualizar establecimiento               

                if (!IsNullOrEmpty(bulkInsertDto.CatEstablecimiento))
                {
                    foreach (var item in bulkInsertDto.CatEstablecimiento)
                    {
                        //var existing = await context.CatEstablecimiento
                        //    .FindAsync(item.IdCatEstablecimiento);
                        //var existing = await context.EstablecimientoCanasta
                        //    .Where(ec => ec.IdEstablecimientoCanasta == item.IdCatEstablecimiento)
                        //    .Include(ec => ec.CatEstablecimiento)
                        //    .Select(ec => ec.CatEstablecimiento)
                        //    .FirstOrDefaultAsync();
                        //var ec = await context.EstablecimientoCanasta
                        //      .FirstOrDefaultAsync(ec => ec.IdEstablecimientoCanasta == item.IdCatEstablecimiento);
                        //if (ec != null)
                        //{
                        //    var existing = await context.CatEstablecimiento.FindAsync(ec.ObjIdCatEstablecimiento);
                        //    // Resto de la lógica...
                        //}

                        // Realizar el join y filtrar por IdEstablecimientoCanasta
                        var existing = await context.EstablecimientoCanasta
                            .Where(ec => ec.IdEstablecimientoCanasta == item.IdCatEstablecimiento)
                            .Select(ec => ec.CatEstablecimiento) // Asumiendo que tienes una propiedad de navegación
                            .FirstOrDefaultAsync();

                        if (existing != null)
                        {
                            // Actualizar las propiedades necesarias
                            existing.Encargado = item.Encargado;
                            existing.Cargo = item.Cargo;
                            existing.Telefono = item.Telefono;
                            existing.Direccion = item.Direccion;
                            existing.CoordenadaX = item.CoordenadaX;
                            existing.CoordenadaY = item.CoordenadaY;
                        }
                    }

                    await context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return Ok(new { message = "Datos procesados exitosamente." });
            }
            catch (SqlException sqlEx)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error de base de datos: {sqlEx.Message}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        private static bool IsNullOrEmpty<T>(IEnumerable<T>? collection) =>
            collection == null || !collection.Any();

        private string jsonRetorno(int Retorno, string Mensaje, bool Resultado, string Valor = "NINGUNO")
        {
            var jsonMensaje = "{" + "\"" + "Mensaje" + "\"" + ": " + "\"" + Mensaje + "\"" + ", " + "\"" + "Retorno" + "\"" + ": " + "\"" + Retorno.ToString() + "\"" + ", " + "\"" + "Resultado" + "\"" + ": " + "\"" + Resultado.ToString().ToLower() + "\"" + ", " + "\"" + "Valor" + "\"" + ": " + Valor + "}";
            return jsonMensaje;
        }
    }
}
