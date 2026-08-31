using Alexa.DAL.Cenagro;
using Alexa.DAL.IPP;
using Alexa.DTOs;
using Alexa.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using System.Data;
using System.Globalization;
using System.Linq;

namespace Alexa.Controllers
{
    [ApiController]
    [Route("endpoint/cipp")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class CippController : ControllerBase
    {
        private readonly IppDbContext context;
        private readonly IConfiguration configuration;
        // 1. Inyecta IDbContextFactory<Alexa.IppDbContext> en tu servicio/controlador en lugar de DbContext directamente
        private readonly IDbContextFactory<IppDbContext> _contextFactory;

        public CippController(IppDbContext context, IConfiguration configuration, IDbContextFactory<IppDbContext> contextFactory)
        {
            this.context = context;
            this.configuration = configuration;
            _contextFactory = contextFactory;
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

        //[NoCache]
        //[HttpGet("Muestra1/{empleado}")]
        //public async Task<IActionResult> MuestrasFiltradas1([FromRoute] string empleado)
        //{
        //    try
        //    {
        //        // Validar que el parámetro "empleado" no esté vacío
        //        if (string.IsNullOrEmpty(empleado))
        //        {
        //            return BadRequest("El parámetro 'empleado' es requerido.");
        //        }

        //        // Construir la consulta LINQ
        //        // Usamos AsNoTracking porque es una consulta de solo lectura (Performance boost)
        //        //var muestra = await context.Muestra
        //        //    .AsNoTracking()
        //        //    .Where(ei => ei.Activo)
        //        //    .Join(context.AsignacionPersonal.Where(m => m.Activo),
        //        //        ei => ei.ObjIdEstablecimientoCanasta,
        //        //        m => m.ObjIdEstablecimientoCanasta,
        //        //        (ei, m) => new { ei, m })
        //        //    .Join(context.Usuario.Where(l => l.Activo && l.UsuarioId == empleado),
        //        //        combined => combined.m.ObjIdCatPersonal,
        //        //        l => l.ID_EMPLEADO,
        //        //        (combined, l) => new { combined.ei, combined.m, l })
        //        //    .Join(context.EstablecimientoCanasta,
        //        //        combined => combined.ei.ObjIdEstablecimientoCanasta,
        //        //        p => p.IdEstablecimientoCanasta,
        //        //        (combined, p) => new { combined.ei, combined.l, p })
        //        //    .Join(context.CatEstablecimiento.Where(e => e.Activo),
        //        //        combined => combined.p.ObjIdCatEstablecimiento,
        //        //        e => e.IdCatEstablecimiento,
        //        //        (combined, e) => new { combined.ei, combined.l, combined.p, e })
        //        //    .Join(context.CatVariedad,
        //        //        combined => combined.ei.ObjIdCatVariedad,
        //        //        v => v.IdCatVariedad,
        //        //        (combined, v) => new { combined.ei, combined.l, combined.p, combined.e, v })
        //        //    .Join(context.CatCanasta.Where(c => c.Activo && c.ObjIdCatEncuesta == 2),
        //        //        combined => combined.p.ObjIdCatCanasta,
        //        //        c => c.IdCatCanasta,
        //        //        (combined, c) => new
        //        //        {
        //        //            combined.ei.IdMuestra,
        //        //            combined.ei.ObjIdEstablecimientoCanasta,
        //        //            combined.ei.ObjIdCatVariedad,
        //        //            combined.ei.ObjIdDia,
        //        //            combined.ei.Detalle,
        //        //            combined.ei.NVeces,
        //        //            NombreEstablecimiento = combined.e.Nombre,
        //        //            combined.v.IdCatVariedad,
        //        //            NombreVariedad = combined.v.Descripcion,
        //        //            combined.p.ObjIdCatCanasta,
        //        //            NombreCanasta = c.Nombre // Si es Inner Join, c nunca será null
        //        //        })
        //        //    .ToListAsync();


        //        ////aqui
        //        //var muestra =  (from ei in context.Muestra
        //        //                     join m in context.AsignacionPersonal on ei.ObjIdEstablecimientoCanasta equals m.ObjIdEstablecimientoCanasta
        //        //                     join l in context.Usuario on m.ObjIdCatPersonal equals l.ID_EMPLEADO   //context.SEC_EMPLEADO on m.ObjIdCatPersonal equals l.ID_EMPLEADO
        //        //                     join p in context.EstablecimientoCanasta on ei.ObjIdEstablecimientoCanasta equals p.IdEstablecimientoCanasta
        //        //                     join e in context.CatEstablecimiento on p.ObjIdCatEstablecimiento equals e.IdCatEstablecimiento
        //        //                     join v in context.CatVariedad on ei.ObjIdCatVariedad equals v.IdCatVariedad
        //        //                     join c in context.CatCanasta on p.ObjIdCatCanasta equals c.IdCatCanasta
        //        //                     where ei.Activo && l.Activo && m.Activo && l.UsuarioId == empleado //l.USERLOGIN == empleado 
        //        //                     && l.Activo && e.Activo && c.ObjIdCatEncuesta == 2 && c.Activo
        //        //                     select new
        //        //                     {
        //        //                         ei.IdMuestra,
        //        //                         ei.ObjIdEstablecimientoCanasta,
        //        //                         ei.ObjIdCatVariedad,
        //        //                         ei.ObjIdDia,
        //        //                         ei.Detalle,
        //        //                         ei.NVeces,
        //        //                         NombreEstablecimiento = e.Nombre,
        //        //                         v.IdCatVariedad,
        //        //                         NombreVariedad = v.Descripcion,
        //        //                         p.ObjIdCatCanasta,
        //        //                         NombreCanasta = c != null ? c.Nombre : null //(string)
        //        //                     }).AsNoTracking().ToListAsync(); // Materializar la consulta                

        //        //var establecimiento =  (from ei in context.Muestra
        //        //                             join m in context.AsignacionPersonal on ei.ObjIdEstablecimientoCanasta equals m.ObjIdEstablecimientoCanasta
        //        //                             join l in context.Usuario on m.ObjIdCatPersonal equals l.ID_EMPLEADO  //context.SEC_EMPLEADO on m.ObjIdCatPersonal equals l.ID_EMPLEADO
        //        //                             join p in context.EstablecimientoCanasta on ei.ObjIdEstablecimientoCanasta equals p.IdEstablecimientoCanasta
        //        //                             join e in context.CatEstablecimiento on p.ObjIdCatEstablecimiento equals e.IdCatEstablecimiento
        //        //                             join c in context.CatCanasta on p.ObjIdCatCanasta equals c.IdCatCanasta
        //        //                             where ei.Activo && m.Activo && l.Activo && l.UsuarioId == empleado //l.USERLOGIN == empleado 
        //        //                             && e.Activo && c.Activo && e.Activo && c.ObjIdCatEncuesta == 2
        //        //                             select new
        //        //                             {
        //        //                                 c.IdCatCanasta,
        //        //                                 p.IdEstablecimientoCanasta,
        //        //                                 e.IdCatEstablecimiento,
        //        //                                 e.ObjCodMuni,
        //        //                                 e.Razon_soc,
        //        //                                 NombreEstablecimiento = e.Nombre,
        //        //                                 e.Encargado,
        //        //                                 e.Cargo,
        //        //                                 e.Telefono,
        //        //                                 e.Direccion,
        //        //                                 e.DiaHabil,
        //        //                                 FechaDefinidaRecoleccion = (from ec in context.EstablecimientoCanasta
        //        //                                                             join ce in context.CatEstablecimiento on ec.ObjIdCatEstablecimiento equals ce.IdCatEstablecimiento
        //        //                                                             join cal in context.CatCalendario on ce.DiaHabil equals Convert.ToInt32(cal.DiaLaboral)
        //        //                                                             where ec.IdEstablecimientoCanasta == p.IdEstablecimientoCanasta && ce.Activo
        //        //                                                             && cal.Fecha.Year == DateTime.Now.Year
        //        //                                                             && cal.Fecha.Month == DateTime.Now.Month
        //        //                                                             select cal.Fecha).FirstOrDefault()
        //        //                             }).AsNoTracking().Distinct().ToListAsync(); // Materializar la consulta

        //        var muestraTask = Task.Run(async () =>
        //        {
        //            using var ctx = _contextFactory.CreateDbContext();
        //            return await (from ei in ctx.Muestra
        //                          join m in ctx.AsignacionPersonal on ei.ObjIdEstablecimientoCanasta equals m.ObjIdEstablecimientoCanasta
        //                          join l in ctx.Usuario on m.ObjIdCatPersonal equals l.ID_EMPLEADO
        //                          join p in ctx.EstablecimientoCanasta on ei.ObjIdEstablecimientoCanasta equals p.IdEstablecimientoCanasta
        //                          join e in ctx.CatEstablecimiento on p.ObjIdCatEstablecimiento equals e.IdCatEstablecimiento
        //                          join v in ctx.CatVariedad on ei.ObjIdCatVariedad equals v.IdCatVariedad
        //                          join c in ctx.CatCanasta on p.ObjIdCatCanasta equals c.IdCatCanasta
        //                          where ei.Activo && l.Activo && m.Activo && l.UsuarioId == empleado
        //                                && e.Activo && c.ObjIdCatEncuesta == 2 && c.Activo
        //                          select new
        //                          {
        //                              ei.IdMuestra,
        //                              ei.ObjIdEstablecimientoCanasta,
        //                              ei.ObjIdCatVariedad,
        //                              ei.ObjIdDia,
        //                              ei.Detalle,
        //                              ei.NVeces,
        //                              NombreEstablecimiento = e.Nombre,
        //                              v.IdCatVariedad,
        //                              NombreVariedad = v.Descripcion,
        //                              p.ObjIdCatCanasta,
        //                              NombreCanasta = c != null ? c.Nombre : null
        //                          }).AsNoTracking().ToListAsync();
        //        });

        //        var establecimientoTask = Task.Run(async () =>
        //        {
        //            using var ctx = _contextFactory.CreateDbContext();
        //            return await (from ei in ctx.Muestra
        //                          join m in ctx.AsignacionPersonal on ei.ObjIdEstablecimientoCanasta equals m.ObjIdEstablecimientoCanasta
        //                          join l in ctx.Usuario on m.ObjIdCatPersonal equals l.ID_EMPLEADO
        //                          join p in ctx.EstablecimientoCanasta on ei.ObjIdEstablecimientoCanasta equals p.IdEstablecimientoCanasta
        //                          join e in ctx.CatEstablecimiento on p.ObjIdCatEstablecimiento equals e.IdCatEstablecimiento
        //                          join c in ctx.CatCanasta on p.ObjIdCatCanasta equals c.IdCatCanasta
        //                          where ei.Activo && m.Activo && l.Activo && l.UsuarioId == empleado
        //                                && e.Activo && c.Activo && c.ObjIdCatEncuesta == 2
        //                          select new
        //                          {
        //                              c.IdCatCanasta,
        //                              p.IdEstablecimientoCanasta,
        //                              e.IdCatEstablecimiento,
        //                              e.ObjCodMuni,
        //                              e.Razon_soc,
        //                              NombreEstablecimiento = e.Nombre,
        //                              e.Encargado,
        //                              e.Cargo,
        //                              e.Telefono,
        //                              e.Direccion,
        //                              e.DiaHabil,
        //                              FechaDefinidaRecoleccion = (from ec in ctx.EstablecimientoCanasta
        //                                                          join ce in ctx.CatEstablecimiento on ec.ObjIdCatEstablecimiento equals ce.IdCatEstablecimiento
        //                                                          join cal in ctx.CatCalendario on ce.DiaHabil equals Convert.ToInt32(cal.DiaLaboral)
        //                                                          where ec.IdEstablecimientoCanasta == p.IdEstablecimientoCanasta && ce.Activo
        //                                                                && cal.Fecha.Year == DateTime.Now.Year
        //                                                                && cal.Fecha.Month == DateTime.Now.Month
        //                                                          select cal.Fecha).FirstOrDefault()
        //                          }).AsNoTracking().Distinct().ToListAsync();
        //        });

        //        await Task.WhenAll(muestraTask, establecimientoTask); 

        //        var muestra = await muestraTask;
        //        var establecimiento = await establecimientoTask;

        //        // Eliminar duplicados si es necesario (opcional)
        //        //var establecimientosUnicos = establecimiento
        //        //    .GroupBy(x => x.IdEstablecimientoCanasta)
        //        //    .Select(g => g.First())
        //        //    .ToList();


        //        //var variedad = await (from ei in context.Muestra
        //        //                      join m in context.AsignacionPersonal on ei.ObjIdEstablecimientoCanasta equals m.ObjIdEstablecimientoCanasta
        //        //                      join l in context.SEC_EMPLEADO on m.ObjIdCatPersonal equals l.ID_EMPLEADO
        //        //                      join p in context.EstablecimientoCanasta on ei.ObjIdEstablecimientoCanasta equals p.IdEstablecimientoCanasta
        //        //                      join e in context.CatEstablecimiento on p.ObjIdCatEstablecimiento equals e.IdCatEstablecimiento
        //        //                      join v in context.CatVariedad on ei.ObjIdCatVariedad equals v.IdCatVariedad
        //        //                      from c in context.CatCanasta
        //        //                          .Where(x => x.IdCatCanasta == p.ObjIdCatCanasta)
        //        //                          .DefaultIfEmpty() // LEFT JOIN
        //        //                      where ei.Activo && l.Activo && c.Activo && l.ID_EMPLEADO == empleado && c.ObjIdCatEncuesta == 2
        //        //                      select new
        //        //                      {
        //        //                          v.IdCatVariedad,
        //        //                          ei.ObjIdEstablecimientoCanasta,
        //        //                          v.ObjIdCatCanasta,
        //        //                          NombreVariedad = v.Descripcion
        //        //                      }).ToListAsync(); // Materializar la consulta

        //        // Devolver la respuesta con ambos objetos los resultados
        //        return Ok(new
        //        {
        //            Muestra = muestra,
        //            Establecimiento = establecimiento
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        // Manejar errores y devolver una respuesta de error
        //        return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        //    }
        //}

        [NoCache]
        [HttpGet("Muestra/{empleado}")]
        public async Task<IActionResult> MuestrasFiltradas([FromRoute] string empleado)
        {
            try
            {
                if (string.IsNullOrEmpty(empleado))
                {
                    return BadRequest("El parámetro 'empleado' es requerido.");
                }

                // 1. Capturamos las variables de fecha fuera de la query de LINQ
                var currentYear = DateTime.Now.Year;
                var currentMonth = DateTime.Now.Month;

                // 2. Eliminamos Task.Run innecesario y usamos directamente el contexto
                using var ctx = _contextFactory.CreateDbContext();

                var muestraTask = Task.Run(async () =>
                {
                    using var ctx = _contextFactory.CreateDbContext();
                    return await (from ei in ctx.Muestra
                                  join m in ctx.AsignacionPersonal on ei.ObjIdEstablecimientoCanasta equals m.ObjIdEstablecimientoCanasta
                                  join l in ctx.Usuario on m.ObjIdCatPersonal equals l.ID_EMPLEADO
                                  join p in ctx.EstablecimientoCanasta on ei.ObjIdEstablecimientoCanasta equals p.IdEstablecimientoCanasta
                                  join e in ctx.CatEstablecimiento on p.ObjIdCatEstablecimiento equals e.IdCatEstablecimiento
                                  join v in ctx.CatVariedad on ei.ObjIdCatVariedad equals v.IdCatVariedad
                                  join c in ctx.CatCanasta on p.ObjIdCatCanasta equals c.IdCatCanasta
                                  where ei.Activo && l.Activo && m.Activo && l.UsuarioId == empleado
                                        && e.Activo && !e.TipoGrande && c.ObjIdCatEncuesta == 2 && c.Activo
                                  select new
                                  {
                                      ei.IdMuestra,
                                      ei.ObjIdEstablecimientoCanasta,
                                      ei.ObjIdCatVariedad,
                                      v.Codigo,
                                      ei.ObjIdDia,
                                      ei.Detalle,
                                      ei.NVeces,
                                      NombreEstablecimiento = e.Nombre,
                                      v.IdCatVariedad,
                                      NombreVariedad = v.Descripcion,
                                      p.ObjIdCatCanasta,
                                      NombreCanasta = c != null ? c.Nombre : null
                                  }).AsNoTracking().ToListAsync();
                });

                var establecimientoTask = Task.Run(async () =>
                {
                    using var ctx = _contextFactory.CreateDbContext();
                    return await (from ei in ctx.Muestra
                                             join m in ctx.AsignacionPersonal on ei.ObjIdEstablecimientoCanasta equals m.ObjIdEstablecimientoCanasta
                                             join l in ctx.Usuario on m.ObjIdCatPersonal equals l.ID_EMPLEADO
                                             join p in ctx.EstablecimientoCanasta on ei.ObjIdEstablecimientoCanasta equals p.IdEstablecimientoCanasta
                                             join e in ctx.CatEstablecimiento on p.ObjIdCatEstablecimiento equals e.IdCatEstablecimiento
                                             join c in ctx.CatCanasta on p.ObjIdCatCanasta equals c.IdCatCanasta
                                             where ei.Activo && m.Activo && l.Activo && l.UsuarioId == empleado
                                                   && e.Activo && !e.TipoGrande && c.Activo && c.ObjIdCatEncuesta == 2
                                             select new
                                             {
                                                 c.IdCatCanasta,
                                                 p.IdEstablecimientoCanasta,
                                                 e.Codigo,
                                                 e.IdCatEstablecimiento,
                                                 e.ObjCodMuni,
                                                 e.Razon_soc,
                                                 NombreEstablecimiento = e.Nombre,
                                                 e.Encargado,
                                                 e.Cargo,
                                                 e.Telefono,
                                                 e.Direccion,
                                                 e.DiaHabil,
                                                 // SOLUCIÓN: Casteamos el resultado a DateTime? para evitar el COALESCE inválido
                                                 FechaDefinidaRecoleccion = (from ec in ctx.EstablecimientoCanasta
                                                                             join ce in ctx.CatEstablecimiento on ec.ObjIdCatEstablecimiento equals ce.IdCatEstablecimiento
                                                                             // Se asume que cal.DiaLaboral se puede comparar o mapear correctamente sin Convert.ToInt32
                                                                             join cal in ctx.CatCalendario on ce.DiaHabil.ToString() equals cal.DiaLaboral
                                                                             where ec.IdEstablecimientoCanasta == p.IdEstablecimientoCanasta && ce.Activo
                                                                                  && cal.Fecha.Year == currentYear
                                                                                  && cal.Fecha.Month == currentMonth
                                                                             select (DateTime?)cal.Fecha).FirstOrDefault()
                                             }).AsNoTracking().Distinct().ToListAsync();
                });

                await Task.WhenAll(muestraTask, establecimientoTask);  

                var muestra = await muestraTask;
                var establecimiento = await establecimientoTask;

                    return Ok(new
                    {
                        Muestra = muestra,
                        Establecimiento = establecimiento
                    });
                }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [NoCache]
        [HttpGet("Validamuestra/{empleado}/{canasta}/{municipio}")]
        public async Task<IActionResult> Validamuestra([FromRoute] string empleado, [FromRoute] int canasta, [FromRoute] int municipio)
        {
            // Obtener el día, mes y año actuales
            var (currentDay, currentMonth, currentYear) = GetCurrentDate();

            // Crear la variable como cadena concatenada:
            string variable = currentMonth.ToString() + currentYear.ToString();
            var querys = await (from ei in context.Muestra
                                join m in context.AsignacionPersonal on ei.ObjIdEstablecimientoCanasta equals m.ObjIdEstablecimientoCanasta
                                join l in context.Usuario on m.ObjIdCatPersonal equals l.ID_EMPLEADO //context.SEC_EMPLEADO on m.ObjIdCatPersonal equals l.ID_EMPLEADO
                                join p in context.EstablecimientoCanasta on ei.ObjIdEstablecimientoCanasta equals p.IdEstablecimientoCanasta
                                join e in context.CatEstablecimiento on p.ObjIdCatEstablecimiento equals e.IdCatEstablecimiento
                                join v in context.CatVariedad on ei.ObjIdCatVariedad equals v.IdCatVariedad
                                join c in context.CatCanasta on p.ObjIdCatCanasta equals c.IdCatCanasta
                                where ei.Activo && m.Activo && l.Activo && l.UsuarioId == empleado //l.USERLOGIN == empleado 
                                && p.ObjIdCatCanasta == canasta
                                    && e.Activo && !e.TipoGrande && e.ObjCodMuni == municipio
                                    && c.ObjIdCatEncuesta == 2 && c.Activo
                                    && !context.Detalle.Any(sp =>
                                        sp.ObjIdCatCanasta == canasta && sp.ObjCodMuni == municipio &&
                                        sp.ObjIdEstablecimientoCanasta == ei.ObjIdEstablecimientoCanasta &&
                                        sp.ObjIdCatVariedad == ei.ObjIdCatVariedad &&
                                        sp.muestraid == variable
                                        //sp.FechaDefinidaRecoleccion.Year == DateTime.Now.Year &&
                                        //sp.FechaDefinidaRecoleccion.Month == DateTime.Now.Month
                                        )
                                select new
                                {
                                    p.ObjIdCatCanasta,
                                    e.ObjCodMuni,
                                    ei.ObjIdEstablecimientoCanasta,
                                    NombreEstablecimiento = e.Nombre,
                                    NombreVariedad = v.Descripcion,
                                    NombreCanasta = c != null ? c.Nombre : null //(string)
                                }).AsNoTracking().ToListAsync(); // Materializar la consulta                  

            // Establecer encabezados para evitar caché
            //Response.Headers["Cache-Control"] = "no-store";
            //Response.Headers["Pragma"] = "no-cache";

            if (querys.Count > 0)
            {
                return Ok(querys);
            }

            //return Ok(new { mensaje = "No tiene variedades pendientes." });
            return Ok(querys);
        }
                
        [NoCache]
        [HttpGet("ValidamuestraDia/{empleado}/{canasta}/{municipio}/{dia}")]
        public async Task<IActionResult> ValidamuestraDia([FromRoute] string empleado, [FromRoute] int canasta, [FromRoute] int municipio, [FromRoute] int dia)
        {
            // Obtener el día, mes y año actuales
            var (currentDay, currentMonth, currentYear) = GetCurrentDate();

            // Crear la variable como cadena concatenada:
            string variable = currentMonth.ToString() + currentYear.ToString();
            var querys = await (from ei in context.Muestra
                                join m in context.AsignacionPersonal on ei.ObjIdEstablecimientoCanasta equals m.ObjIdEstablecimientoCanasta
                                join l in context.Usuario on m.ObjIdCatPersonal equals l.ID_EMPLEADO //context.SEC_EMPLEADO on m.ObjIdCatPersonal equals l.ID_EMPLEADO
                                join p in context.EstablecimientoCanasta on ei.ObjIdEstablecimientoCanasta equals p.IdEstablecimientoCanasta
                                join e in context.CatEstablecimiento on p.ObjIdCatEstablecimiento equals e.IdCatEstablecimiento
                                join v in context.CatVariedad on ei.ObjIdCatVariedad equals v.IdCatVariedad
                                join c in context.CatCanasta on p.ObjIdCatCanasta equals c.IdCatCanasta
                                where ei.Activo && m.Activo && l.Activo && l.UsuarioId == empleado //l.USERLOGIN == empleado 
                                && p.ObjIdCatCanasta == canasta
                                    && e.Activo && !e.TipoGrande && e.ObjCodMuni == municipio && e.DiaHabil == dia
                                    && c.ObjIdCatEncuesta == 2 && c.Activo
                                    && !context.Detalle.Any(sp =>
                                        sp.ObjIdCatCanasta == canasta && sp.ObjCodMuni == municipio &&
                                        sp.ObjIdEstablecimientoCanasta == ei.ObjIdEstablecimientoCanasta &&
                                        sp.ObjIdCatVariedad == ei.ObjIdCatVariedad &&
                                        sp.muestraid == variable
                                        //sp.FechaDefinidaRecoleccion.Year == DateTime.Now.Year &&
                                        //sp.FechaDefinidaRecoleccion.Month == DateTime.Now.Month
                                        )
                                select new
                                {
                                    p.ObjIdCatCanasta,
                                    e.ObjCodMuni,
                                    ei.ObjIdEstablecimientoCanasta,
                                    NombreEstablecimiento = e.Nombre,
                                    NombreVariedad = v.Descripcion,
                                    NombreCanasta = c != null ? c.Nombre : null //(string)
                                }).AsNoTracking().ToListAsync(); // Materializar la consulta                  

            // Establecer encabezados para evitar caché
            //Response.Headers["Cache-Control"] = "no-store";
            //Response.Headers["Pragma"] = "no-cache";

            if (querys.Count > 0)
            {
                return Ok(querys);
            }

            //return Ok(new { mensaje = "No tiene variedades pendientes." });
            return Ok(querys);
        }

        [NoCache]
        [HttpGet("ValidamuestraDestiempo/{empleado}/{canasta}/{municipio}/{muestraid}")]
        public async Task<IActionResult> ValidamuestraDestiempo(
            [FromRoute] string empleado,
            [FromRoute] int canasta,
            [FromRoute] int municipio,
            [FromRoute] string muestraid)
        {
            // 1. Validar y extraer Mes y Año desde muestraid (Ej: "62026" o "102026")
            if (string.IsNullOrWhiteSpace(muestraid) || muestraid.Length < 5)
            {
                return BadRequest(new { mensaje = "El formato de muestraid no es válido." });
            }

            // El año siempre son los últimos 4 caracteres
            string stringAnio = muestraid[^4..];
            // El mes es todo lo que está antes de los últimos 4 caracteres
            string stringMes = muestraid[..^4];

            if (!int.TryParse(stringAnio, out int anioFiltro) || !int.TryParse(stringMes, out int mesFiltro))
            {
                return BadRequest(new { mensaje = "El parámetro muestraid no contiene un mes o año válidos." });
            }

            // 2. Consulta optimizada usando las variables locales
            var querys = await (from ei in context.MuestraM
                                join m in context.AsignacionPersonal on ei.ObjIdEstablecimientoCanasta equals m.ObjIdEstablecimientoCanasta
                                join l in context.Usuario on m.ObjIdCatPersonal equals l.ID_EMPLEADO
                                join p in context.EstablecimientoCanasta on ei.ObjIdEstablecimientoCanasta equals p.IdEstablecimientoCanasta
                                join e in context.CatEstablecimiento on p.ObjIdCatEstablecimiento equals e.IdCatEstablecimiento
                                join v in context.CatVariedad on ei.ObjIdCatVariedad equals v.IdCatVariedad
                                join c in context.CatCanasta on p.ObjIdCatCanasta equals c.IdCatCanasta
                                where ei.Activo
                                      && ei.muestraid == muestraid
                                      && m.Activo
                                      && l.Activo
                                      && l.UsuarioId == empleado
                                      && p.ObjIdCatCanasta == canasta
                                      && e.Activo
                                      && !e.TipoGrande
                                      && e.ObjCodMuni == municipio
                                      && c.ObjIdCatEncuesta == 2
                                      && c.Activo
                                      && !context.Detalle.Any(sp =>
                                          sp.ObjIdCatCanasta == canasta
                                          && sp.ObjCodMuni == municipio
                                          && sp.ObjIdEstablecimientoCanasta == ei.ObjIdEstablecimientoCanasta
                                          && sp.ObjIdCatVariedad == ei.ObjIdCatVariedad
                                          && sp.FechaDefinidaRecoleccion.Year == anioFiltro
                                          && sp.FechaDefinidaRecoleccion.Month == mesFiltro)
                                select new
                                {
                                    p.ObjIdCatCanasta,
                                    e.ObjCodMuni,
                                    e.Codigo,
                                    CodVariedad = v.Codigo,
                                    ei.ObjIdEstablecimientoCanasta,
                                    NombreEstablecimiento = e.Nombre,
                                    NombreVariedad = v.Descripcion,
                                    NombreCanasta = c != null ? c.Nombre : null
                                })
                                .AsNoTracking()
                                .ToListAsync();

            return Ok(querys);
        }

        //[NoCache]
        //[HttpGet("ValidamuestraDestiempo1/{empleado}/{canasta}/{municipio}/{muestraid}")]
        //public async Task<IActionResult> ValidamuestraDestiempo1([FromRoute] string empleado, [FromRoute] int canasta, [FromRoute] int municipio, string muestraid)
        //{            
        //    var querys = await (from ei in context.MuestraM
        //                        join m in context.AsignacionPersonal on ei.ObjIdEstablecimientoCanasta equals m.ObjIdEstablecimientoCanasta
        //                        join l in context.Usuario on m.ObjIdCatPersonal equals l.ID_EMPLEADO
        //                        join p in context.EstablecimientoCanasta on ei.ObjIdEstablecimientoCanasta equals p.IdEstablecimientoCanasta
        //                        join e in context.CatEstablecimiento on p.ObjIdCatEstablecimiento equals e.IdCatEstablecimiento
        //                        join v in context.CatVariedad on ei.ObjIdCatVariedad equals v.IdCatVariedad
        //                        join c in context.CatCanasta on p.ObjIdCatCanasta equals c.IdCatCanasta
        //                        where ei.Activo && ei.muestraid == muestraid && m.Activo && l.Activo && l.UsuarioId == empleado
        //                        && p.ObjIdCatCanasta == canasta
        //                            && e.Activo && e.ObjCodMuni == municipio
        //                            && c.ObjIdCatEncuesta == 2 && c.Activo
        //                            && !context.Detalle.Any(sp =>
        //                                sp.ObjIdCatCanasta == canasta && sp.ObjCodMuni == municipio &&
        //                                sp.ObjIdEstablecimientoCanasta == ei.ObjIdEstablecimientoCanasta &&
        //                                sp.ObjIdCatVariedad == ei.ObjIdCatVariedad &&
        //                                sp.FechaDefinidaRecoleccion.Year == DateTime.Now.Year &&
        //                                sp.FechaDefinidaRecoleccion.Month == DateTime.Now.Month)
        //                        select new
        //                        {
        //                            p.ObjIdCatCanasta,
        //                            e.ObjCodMuni,
        //                            ei.ObjIdEstablecimientoCanasta,
        //                            NombreEstablecimiento = e.Nombre,
        //                            NombreVariedad = v.Descripcion,
        //                            NombreCanasta = c != null ? c.Nombre : null //(string)
        //                        }).AsNoTracking().ToListAsync(); // Materializar la consulta                  

        //    // Establecer encabezados para evitar caché
        //    //Response.Headers["Cache-Control"] = "no-store";
        //    //Response.Headers["Pragma"] = "no-cache";

        //    if (querys.Count > 0)
        //    {
        //        return Ok(querys);
        //    }

        //    //return Ok(new { mensaje = "No tiene variedades pendientes." });
        //    return Ok(querys);
        //}

        [NoCache]
        [HttpGet("ValidamuestraDestiempoTodo/{empleado}/{canasta}/{muestraid}")]
        public async Task<IActionResult> ValidamuestraDestiempoTodo(
            [FromRoute] string empleado,
            [FromRoute] int canasta,
            [FromRoute] int municipio,
            [FromRoute] string muestraid)
        {
            // 1. Validar y extraer Mes y Año desde muestraid (Ej: "62026" o "102026")
            if (string.IsNullOrWhiteSpace(muestraid) || muestraid.Length < 5)
            {
                return BadRequest(new { mensaje = "El formato de muestraid no es válido." });
            }

            // El año siempre son los últimos 4 caracteres
            string stringAnio = muestraid[^4..];
            // El mes es todo lo que está antes de los últimos 4 caracteres
            string stringMes = muestraid[..^4];

            if (!int.TryParse(stringAnio, out int anioFiltro) || !int.TryParse(stringMes, out int mesFiltro))
            {
                return BadRequest(new { mensaje = "El parámetro muestraid no contiene un mes o año válidos." });
            }

            // 2. Consulta optimizada usando las variables locales
            var querys = await (from ei in context.MuestraM
                                join m in context.AsignacionPersonal on ei.ObjIdEstablecimientoCanasta equals m.ObjIdEstablecimientoCanasta
                                join l in context.Usuario on m.ObjIdCatPersonal equals l.ID_EMPLEADO
                                join p in context.EstablecimientoCanasta on ei.ObjIdEstablecimientoCanasta equals p.IdEstablecimientoCanasta
                                join e in context.CatEstablecimiento on p.ObjIdCatEstablecimiento equals e.IdCatEstablecimiento
                                join v in context.CatVariedad on ei.ObjIdCatVariedad equals v.IdCatVariedad
                                join c in context.CatCanasta on p.ObjIdCatCanasta equals c.IdCatCanasta
                                where ei.Activo
                                      && ei.muestraid == muestraid
                                      && m.Activo
                                      && l.Activo
                                      && l.UsuarioId == empleado
                                      && p.ObjIdCatCanasta == canasta
                                      && e.Activo
                                      && !e.TipoGrande
                                      //&& e.ObjCodMuni == municipio
                                      && c.ObjIdCatEncuesta == 2
                                      && c.Activo
                                      && !context.Detalle.Any(sp =>
                                          sp.ObjIdCatCanasta == canasta
                                         // && sp.ObjCodMuni == municipio
                                          && sp.ObjIdEstablecimientoCanasta == ei.ObjIdEstablecimientoCanasta
                                          && sp.ObjIdCatVariedad == ei.ObjIdCatVariedad
                                          && sp.FechaDefinidaRecoleccion.Year == anioFiltro
                                          && sp.FechaDefinidaRecoleccion.Month == mesFiltro)
                                select new
                                {
                                    p.ObjIdCatCanasta,
                                    e.ObjCodMuni,
                                    e.Codigo,
                                    CodVariedad = v.Codigo,
                                    ei.ObjIdEstablecimientoCanasta,
                                    NombreEstablecimiento = e.Nombre,
                                    NombreVariedad = v.Descripcion,
                                    NombreCanasta = c != null ? c.Nombre : null
                                })
                                .AsNoTracking()
                                .ToListAsync();

            return Ok(querys);
        }
        
        [NoCache]
        [HttpGet("Catalogos/{empleado}")]
        public async Task<IActionResult> CatalogosListados([FromRoute] string empleado)
        {
            var calendario = await context.CatCalendario.AsNoTracking()
               .Where(c => c.Fecha.Year == DateTime.Now.Year)
                .ToListAsync();

            //var municipio = await (from et in context.relUsuarioUbicacionCanasta.Where(u => u.UsuarioId == empleado)
            //                       join m in context.SEC_MUNI on et.Id_Municip equals m.ID_Muni
            //                       select new SEC_MUNIDTO
            //                       {
            //                           ID_Muni = m.ID_Muni,
            //                           NOM_MUNI = m.NOM_MUNI,
            //                           ObjIdCatCanasta = et.IdCatCanasta
            //                       }).OrderBy(c => c.ID_Muni).ToListAsync();

            var municipio = await (from ei in context.Muestra
                                   join m in context.AsignacionPersonal on ei.ObjIdEstablecimientoCanasta equals m.ObjIdEstablecimientoCanasta
                                   join l in context.Usuario on m.ObjIdCatPersonal equals l.ID_EMPLEADO //context.SEC_EMPLEADO on m.ObjIdCatPersonal equals l.ID_EMPLEADO
                                   join t in context.EstablecimientoCanasta on ei.ObjIdEstablecimientoCanasta equals t.IdEstablecimientoCanasta
                                   join n in context.CatEstablecimiento on t.ObjIdCatEstablecimiento equals n.IdCatEstablecimiento
                                   join p in context.CatCanasta on t.ObjIdCatCanasta equals p.IdCatCanasta
                                   join z in context.SEC_MUNI on n.ObjCodMuni equals z.ID_Muni
                                   where ei.Activo && m.Activo && l.Activo && l.UsuarioId == empleado //l.USERLOGIN == empleado
                                          && n.Activo && !n.TipoGrande && p.Activo && p.ObjIdCatEncuesta == 2
                                   group z by new { z.ID_Muni, z.NOM_MUNI, t.ObjIdCatCanasta } into g
                                   select new SEC_MUNIDTO
                                   {
                                       ID_Muni = g.Key.ID_Muni,
                                       NOM_MUNI = g.Key.NOM_MUNI,
                                       ObjIdCatCanasta = g.Key.ObjIdCatCanasta
                                   }).AsNoTracking().OrderBy(c => c.ObjIdCatCanasta).ToListAsync();


            var canasta = await context.CatCanasta.AsNoTracking()
                .Where(c => c.ObjIdCatEncuesta == 2 && c.Activo)
                .ToListAsync();

            var causal = await context.CatValorCatalogo.AsNoTracking()
                .Where(v => new[] { 10, 11, 12, 50, 58 }.Contains(v.IdCatValorCatalogo))
                .ToListAsync();

            var estados = await context.CatValorCatalogo.AsNoTracking()
               .Where(v => new[] { 13, 14, 15, 16 }.Contains(v.IdCatValorCatalogo))
               .ToListAsync();

            var monedas = await context.CatValorCatalogo.AsNoTracking()
               .Where(v => new[] { 42, 43 }.Contains(v.IdCatValorCatalogo))
               .ToListAsync();

            var tipocambios = await context.CatTipoCambio.AsNoTracking()
               .Where(c => c.Fecha.Year == DateTime.Now.Year && c.Fecha.Month == DateTime.Now.Month)
                .ToListAsync();

            var unidadmedida = await (from um in context.CatUMedVar
                                      join cv in context.CatVariedad on um.ObjIdCatVariedad equals cv.IdCatVariedad
                                      join uni in context.CatUnidadMedida on um.ObjURecolId equals uni.IdCatUnidadMedida into uniGroup
                                      from uni in uniGroup.DefaultIfEmpty()
                                      join ei in context.Muestra on um.ObjIdCatVariedad equals ei.ObjIdCatVariedad
                                      join m in context.AsignacionPersonal on ei.ObjIdEstablecimientoCanasta equals m.ObjIdEstablecimientoCanasta
                                      join l in context.Usuario on m.ObjIdCatPersonal equals l.ID_EMPLEADO //context.SEC_EMPLEADO on m.ObjIdCatPersonal equals l.ID_EMPLEADO
                                      join p in context.EstablecimientoCanasta on ei.ObjIdEstablecimientoCanasta equals p.IdEstablecimientoCanasta
                                      join c in context.CatCanasta on p.ObjIdCatCanasta equals c.IdCatCanasta
                                      where ei.Activo && m.Activo && l.Activo && l.UsuarioId == empleado //l.USERLOGIN == empleado
                                            && c.Activo && c.ObjIdCatEncuesta == 2
                                      select new
                                      {
                                          ObjIdCatVariedad = um.ObjIdCatVariedad,
                                          ObjURecolId = um.ObjURecolId,
                                          Nombre = uni.Nombre
                                      }).AsNoTracking().Distinct().ToListAsync();

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
                    from c in context.CatCatalogo.AsNoTracking()
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
                var maxFechasQuery = from d in context.Detalle.AsNoTracking()
                                     join m in context.AsignacionPersonal on d.ObjIdEstablecimientoCanasta equals m.ObjIdEstablecimientoCanasta
                                     join l in context.Usuario on m.ObjIdCatPersonal equals l.ID_EMPLEADO  //context.SEC_EMPLEADO on m.ObjIdCatPersonal equals l.ID_EMPLEADO
                                     where d.FechaImputado == null
                                           && d.FechaDefinidaRecoleccion >= fechaInicioMesAnterior
                                           && d.FechaDefinidaRecoleccion <= fechaFinMesAnterior
                                           //&& d.ObjIdEstablecimientoCanasta == 512
                                           //&& d.ObjIdCatVariedad == 680
                                           && estadosValidosSet.Contains(d.ObjIdEstadoVar)
                                           && l.UsuarioId == empleado //l.USERLOGIN == empleado
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
                var query = from s in maxFechasQuery.AsNoTracking()
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
                            join z in context.Usuario on a.ObjIdCatPersonal equals z.ID_EMPLEADO //context.SEC_EMPLEADO
                            join cv in context.CatValorCatalogo on d.ObjIdEstadoVar equals cv.IdCatValorCatalogo
                            join v in context.CatVariedad on d.ObjIdCatVariedad equals v.IdCatVariedad
                            where m.Activo && z.UsuarioId == empleado //z.USERLOGIN == empleado
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
                                d.TasaCambio,
                                d.Observacion
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

        [NoCache]
        [HttpGet("Previo/{empleado}")]
        public async Task<IActionResult> DatosReferencia([FromRoute] string empleado)
        {
            try
            {
                var hoy = DateTime.Today;

                var codigosExcluidos = new[] { "CD", "CT", "RCH", "OC" };

                // Obtener IDs de estados válidos
                var estadosValidos = await (
                    from c in context.CatCatalogo.AsNoTracking()
                    join v in context.CatValorCatalogo on c.IdCatCatalogo equals v.ObjIdCatCatalogo
                    where c.Codigo == "ESTADOGRABACION"
                          && c.Activo
                          && v.Activo
                            // ✅ CONDICIONES EXPLÍCITAS - Compatible con SQL 2008 R2
                            && v.Codigo != "CD"
                            && v.Codigo != "CT"
                            && v.Codigo != "RCH"
                            && v.Codigo != "OC"
                    //&& !codigosExcluidos.Contains(v.Codigo)
                    select v.IdCatValorCatalogo
                ).ToListAsync();

                // Convertir a HashSet para mejor rendimiento en Contains
                var estadosValidosSet = new HashSet<int>(estadosValidos);

                // Subconsulta: grupo con MAX(FechaDefinidaRecoleccion) donde PrecioRealRecolectado tiene valor y != 0
                // Busca en todo el historial pasado (sin límite al mes anterior), priorizando la última recolección válida
                var maxFechasQuery = from d in context.Detalle.AsNoTracking()
                                     join m in context.AsignacionPersonal on d.ObjIdEstablecimientoCanasta equals m.ObjIdEstablecimientoCanasta
                                     join l in context.Usuario on m.ObjIdCatPersonal equals l.ID_EMPLEADO  //context.SEC_EMPLEADO on m.ObjIdCatPersonal equals l.ID_EMPLEADO
                                     where d.FechaImputado == null
                                           && d.PrecioRealRecolectado.HasValue  // Nueva condición: debe tener valor (no null)
                                           && d.PrecioRealRecolectado != 0      // Y diferente de cero
                                           && d.FechaDefinidaRecoleccion < hoy  // Limitar a fechas pasadas para evitar futuro
                                                                                //&& d.ObjIdEstablecimientoCanasta == 512  // Comentado: mantener generalidad
                                                                                //&& d.ObjIdCatVariedad == 680
                                           && estadosValidosSet.Contains(d.ObjIdEstadoVar)
                                           && m.Activo
                                           && l.UsuarioId == empleado //l.USERLOGIN == empleado
                                           && l.Activo
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
                var query = from s in maxFechasQuery.AsNoTracking()
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
                            //join a in context.AsignacionPersonal on d.ObjIdEstablecimientoCanasta equals a.ObjIdEstablecimientoCanasta
                            //join z in context.SEC_EMPLEADO on a.ObjIdCatPersonal equals z.ID_EMPLEADO //context.Usuario
                            //join z in context.Usuario on a.ObjIdCatPersonal equals z.ID_EMPLEADO
                            join cv in context.CatValorCatalogo on d.ObjIdEstadoVar equals cv.IdCatValorCatalogo
                            join v in context.CatVariedad on d.ObjIdCatVariedad equals v.IdCatVariedad
                            //where m.Activo && z.USERLOGIN == empleado //z.UsuarioId == empleado
                            where m.Activo
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
                                d.TasaCambio,
                                d.Observacion
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

        //[NoCache]
        //[HttpGet("MuestraPre_ObsAntiguo/{empleado}")]
        //public async Task<IActionResult> MuestraPre_ObsAntiguo([FromRoute] string empleado)
        //{
        //    try
        //    {
        //        var hoy = DateTime.Today;
        //        var primerDiaMesActual = new DateTime(hoy.Year, hoy.Month, 1);
        //        var fechaInicioMesAnterior = primerDiaMesActual.AddMonths(-1);
        //        var fechaFinMesAnterior = primerDiaMesActual.AddDays(-1);

        //        var codigosExcluidos = new List<string> { "CD", "CT", "RCH", "OC" };

        //        // 1. Traemos los objetos de la base para filtrar por código en memoria
        //        var estadosBase = await context.CatValorCatalogo
        //            .Where(v => v.Activo &&
        //                        context.CatCatalogo.Any(c => c.IdCatCatalogo == v.ObjIdCatCatalogo && c.Codigo == "ESTADOGRABACION" && c.Activo))
        //            .ToListAsync();

        //        // 2. Filtramos en memoria para obtener solo los IDs que nos interesan
        //        // Esto evita que EF Core 8 genere el OPENJSON que rompe en SQL 2008 R2
        //        var idsFiltrados = estadosBase
        //            .Where(v => !codigosExcluidos.Contains(v.Codigo))
        //            .Select(v => v.IdCatValorCatalogo)
        //            .ToList();                

        //        // 3. Consulta principal
        //        var resultado = await (from d in context.Detalle
        //                               join m in context.AsignacionPersonal on d.ObjIdEstablecimientoCanasta equals m.ObjIdEstablecimientoCanasta
        //                               join l in context.Usuario on m.ObjIdCatPersonal equals l.ID_EMPLEADO
        //                               where d.FechaImputado == null
        //                                     && d.FechaDefinidaRecoleccion >= fechaInicioMesAnterior
        //                                     && d.FechaDefinidaRecoleccion <= fechaFinMesAnterior
        //                                     && idsFiltrados.Contains(d.ObjIdEstadoVar)
        //                                     && l.UsuarioId == empleado
        //                               group d by new
        //                               {
        //                                   d.ObjIdEstablecimientoCanasta,
        //                                   d.ObjIdCatVariedad,
        //                                   d.Observacion
        //                               } into g
        //                               select new
        //                               {
        //                                   g.Key.ObjIdEstablecimientoCanasta,
        //                                   g.Key.ObjIdCatVariedad,
        //                                   g.Key.Observacion                                          
        //                               }).ToListAsync();

        //        return Ok(resultado);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Loguear 'ex' adecuadamente aquí
        //        return StatusCode(500, $"Error interno: {ex.Message}");
        //    }
        //}

        //[NoCache]
        //[HttpGet("MuestraPre_Obs/{empleado}")]
        //public async Task<IActionResult> MuestraPre_Obs([FromRoute] string empleado)
        //{
        //    try
        //    {
        //        var hoy = DateTime.Today;
        //        var primerDiaMesActual = new DateTime(hoy.Year, hoy.Month, 1);
        //        var fechaInicioMesAnterior = primerDiaMesActual.AddMonths(-1);
        //        var fechaFinMesAnterior = primerDiaMesActual.AddDays(-1);

        //        // ✅ FILTRO EXPLÍCITO: Evita que EF Core genere OPENJSON
        //        // En lugar de: !codigosExcluidos.Contains(v.Codigo)
        //        var observacionPrevia = await (from d in context.Detalle
        //                                       join m in context.AsignacionPersonal on d.ObjIdEstablecimientoCanasta equals m.ObjIdEstablecimientoCanasta
        //                                       join l in context.Usuario on m.ObjIdCatPersonal equals l.ID_EMPLEADO
        //                                       join v in context.CatValorCatalogo on d.ObjIdEstadoVar equals v.IdCatValorCatalogo
        //                                       join c in context.CatCatalogo on v.ObjIdCatCatalogo equals c.IdCatCatalogo
        //                                       where
        //                                             //d.FechaImputado == null //&& 
        //                                             d.FechaDefinidaRecoleccion >= fechaInicioMesAnterior
        //                                             && d.FechaDefinidaRecoleccion <= fechaFinMesAnterior
        //                                             && m.Activo
        //                                             && l.Activo
        //                                             && v.Activo
        //                                             && c.Codigo == "ESTADOGRABACION"
        //                                             && c.Activo
        //                                             // ✅ CONDICIONES EXPLÍCITAS - Compatible con SQL 2008 R2
        //                                             && v.Codigo != "CD"
        //                                             && v.Codigo != "CT"
        //                                             && v.Codigo != "RCH"
        //                                             && v.Codigo != "OC"
        //                                             && l.UsuarioId == empleado
        //                                       group d by new
        //                                       {
        //                                           d.ObjIdEstablecimientoCanasta,
        //                                           d.ObjIdCatVariedad,
        //                                           d.Observacion
        //                                       } into g
        //                                       select new
        //                                       {
        //                                           g.Key.ObjIdEstablecimientoCanasta,
        //                                           g.Key.ObjIdCatVariedad,
        //                                           g.Key.Observacion
        //                                       }).AsNoTracking().ToListAsync();

        //        return Ok(observacionPrevia);
        //    }
        //    catch (Exception ex)
        //    {
        //        //_logger.LogError(ex, "Error en MuestraPre_Obs para empleado {Empleado}", empleado);
        //        return StatusCode(500, $"Error interno: {ex.Message}");
        //    }
        //}

        [NoCache]
        [HttpGet("MuestraPre_Completa/{empleado}")]
        public async Task<IActionResult> MuestraPre_Completa([FromRoute] string empleado)
        {
            try
            {
                var hoy = DateTime.Today;
                var primerDiaMesActual = new DateTime(hoy.Year, hoy.Month, 1);
                var fechaInicioMesAnterior = primerDiaMesActual.AddMonths(-1);
                var fechaFinMesAnterior = primerDiaMesActual.AddDays(-1);

                // 1️⃣ Primera consulta: Observaciones previas del mes anterior
                var observacionPrevia = await (from d in context.Detalle
                                               join m in context.AsignacionPersonal on d.ObjIdEstablecimientoCanasta equals m.ObjIdEstablecimientoCanasta
                                               join l in context.Usuario on m.ObjIdCatPersonal equals l.ID_EMPLEADO
                                               join v in context.CatValorCatalogo on d.ObjIdEstadoVar equals v.IdCatValorCatalogo
                                               join c in context.CatCatalogo on v.ObjIdCatCatalogo equals c.IdCatCatalogo
                                               where d.FechaDefinidaRecoleccion >= fechaInicioMesAnterior
                                                  && d.FechaDefinidaRecoleccion <= fechaFinMesAnterior
                                                  && m.Activo
                                                  && l.Activo
                                                  && v.Activo
                                                  && c.Codigo == "ESTADOGRABACION"
                                                  && c.Activo
                                                  && v.Codigo != "CD"
                                                  && v.Codigo != "CT"
                                                  && v.Codigo != "RCH"
                                                  && v.Codigo != "OC"
                                                  && l.UsuarioId == empleado
                                               group d by new
                                               {
                                                   d.ObjIdEstablecimientoCanasta,
                                                   d.ObjIdCatVariedad,
                                                   d.Observacion
                                               } into g
                                               select new ObservacionPreviaDto
                                               {
                                                   ObjIdEstablecimientoCanasta = g.Key.ObjIdEstablecimientoCanasta,
                                                   ObjIdCatVariedad = g.Key.ObjIdCatVariedad,
                                                   Observacion = g.Key.Observacion
                                               })
                                               .AsNoTracking()
                                               .ToListAsync();

                // 2️⃣ Segunda consulta: Estados causales (Compatible con SQL Server 2008 R2)
                var estadosCausales = await (from d in context.Detalle
                                             join ce in context.EstablecimientoCanasta on d.ObjIdEstablecimientoCanasta equals ce.IdEstablecimientoCanasta
                                             join v in context.CatValorCatalogo on d.ObjIdEstadoVar equals v.IdCatValorCatalogo
                                             where d.muestraid == "62026"
                                                && d.UsuarioCreacion == empleado
                                                && v.ObjIdCatCatalogo == 4
                                                && v.Activo
                                                && (d.ObjIdEstadoVar == 10 ||
                                                    d.ObjIdEstadoVar == 11 ||
                                                    d.ObjIdEstadoVar == 12 ||
                                                    d.ObjIdEstadoVar == 13 ||
                                                    d.ObjIdEstadoVar == 50)
                                             group d by new
                                             {
                                                 d.ObjIdEstablecimientoCanasta,
                                                 EstadoNombre = v.Nombre
                                             } into g
                                             select new EstadoCausalDto
                                             {
                                                 ObjIdEstablecimientoCanasta = g.Key.ObjIdEstablecimientoCanasta,
                                                 NombreEstado = g.Key.EstadoNombre
                                             })
                                             .AsNoTracking()
                                             .ToListAsync();

                // 3️⃣ Construcción del DTO de respuesta unificada
                var response = new MuestraPreResponseDto
                {
                    ObservacionesPrevias = observacionPrevia,
                    EstadosCausales = estadosCausales
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error en MuestraPre_Completa para empleado {Empleado}", empleado);
                return StatusCode(500, $"Error interno: {ex.Message}");
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
                               join l in context.Usuario on m.ObjIdCatPersonal equals l.ID_EMPLEADO //context.SEC_EMPLEADO on m.ObjIdCatPersonal equals l.ID_EMPLEADO
                               where ei.Activo && m.Activo && l.Activo && l.UsuarioId == empleado  //l.USERLOGIN == empleado
                               select new
                               {
                                   Usuario = l.UsuarioId, //l.USERLOGIN,
                                   Pass = l.Password // l.PASSWD
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
                IsNullOrEmpty(bulkInsertDto.CatEstablecimiento) &&
                IsNullOrEmpty(bulkInsertDto.Muestra))
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
                            .FindAsync(item.ObjIdEstablecimientoCanasta, item.ObjIdCatVariedad, item.muestraid, item.ObjIdCatCanasta, item.ObjCodMuni); //item.FechaDefinidaRecoleccion,

                        if (existing != null)
                        {
                            existing.PrecioCalculado = item.PrecioCalculado;
                            existing.PrecioRealRecolectado = item.PrecioRealRecolectado;
                            existing.Cantidad = item.Cantidad;
                            existing.FechaRecoleccion = item.FechaRecoleccion;
                            existing.TasaCambio = item.TasaCambio;
                            //existing.GrabadoEnOficina = item.GrabadoEnOficina;
                            existing.ObjIdTipoMoneda = item.ObjIdTipoMoneda;
                            existing.ObjIdEstadoVar = item.ObjIdEstadoVar;
                            existing.ObjIdUnidRecolectada = item.ObjIdUnidRecolectada;
                            existing.Observacion = item.Observacion;
                            existing.UsuarioModificacion = item.UsuarioCreacion;
                            existing.FechaModificacion = DateTime.Now;
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

                // 3. Actualizar muestra
                if (!IsNullOrEmpty(bulkInsertDto.Muestra))
                {
                    //foreach (var item in bulkInsertDto.Muestra)
                    //{
                    //    var existing = await context.Muestra
                    //        .Where(ec => ec.ObjIdEstablecimientoCanasta == item.ObjIdEstablecimientoCanasta && ec.ObjIdCatVariedad == item.ObjIdCatVariedad && ec.Activo == true)
                    //        .FirstOrDefaultAsync();

                    //    if (existing != null)
                    //    {
                    //        // Actualizar las propiedades necesarias
                    //        existing.NVeces = item.NVeces;
                    //        existing.UsuarioModificacion = item.UsuarioModificacion;
                    //        existing.FechaModificacion = DateTime.Now;
                    //    }
                    //}

                    // Cargar muestras activas y filtrar en memoria
                    var existingMuestras = context.Muestra
                        .Where(m => m.Activo == true)  // Filtrar básico en DB
                        .AsEnumerable()  // Cambiar a evaluación en cliente
                        .Where(m => bulkInsertDto.Muestra.Any(item => item.ObjIdEstablecimientoCanasta == m.ObjIdEstablecimientoCanasta &&
                                                                       item.ObjIdCatVariedad == m.ObjIdCatVariedad))
                        .ToList();  // Ejecutar en memoria
                                    // Actualizar en memoria
                    foreach (var item in bulkInsertDto.Muestra)
                    {
                        var existing = existingMuestras.FirstOrDefault(m => m.ObjIdEstablecimientoCanasta == item.ObjIdEstablecimientoCanasta &&
                                                                             m.ObjIdCatVariedad == item.ObjIdCatVariedad);
                        if (existing != null)
                        {
                            existing.NVeces = item.NVeces;
                            existing.UsuarioModificacion = item.UsuarioModificacion;
                            existing.FechaModificacion = DateTime.Now;
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
