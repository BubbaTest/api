using Alexa.DAL.IPC;
using Alexa.DAL.Seguridad;
using Alexa.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Alexa.Controllers
{
    [ApiController]
    [Route("endpoint/cipc")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class CipcController : ControllerBase
    {
        private readonly IpcDbContext context;

        public CipcController(IpcDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SeriesPrecios>>> PreciosSeries()
        {
            return await context.SeriesPrecios.ToListAsync();
        }

        //[HttpGet("Muestra/{empleado}")]
        //public async Task<IActionResult> ListaMuestraRecolector([FromRoute] string empleado)
        //{
        //    DbCommand cmd;

        //    // Definición del comando SQL con parámetros de entrada y salida
        //    string sql = "EXEC [dbo].[listaMuestraRecolector] @Empleado, @Retorno OUTPUT, @Mensaje OUTPUT";

        //    // Crear el comando
        //    cmd = context.Database.GetDbConnection().CreateCommand();
        //    cmd.CommandText = sql;

        //    // Agregar parámetros de entrada
        //    cmd.Parameters.Add(new SqlParameter("@Empleado", SqlDbType.VarChar, 6) { Value = empleado });

        //    // Agregar parámetros de salida
        //    var retornoParam = new SqlParameter("@Retorno", SqlDbType.Int) { Direction = ParameterDirection.Output };
        //    var mensajeParam = new SqlParameter("@Mensaje", SqlDbType.NVarChar, 1024) { Direction = ParameterDirection.Output };

        //    cmd.Parameters.Add(retornoParam);
        //    cmd.Parameters.Add(mensajeParam);

        //    try
        //    {
        //        // Abrir la conexión a la base de datos
        //        await context.Database.OpenConnectionAsync();

        //        // Ejecutar el comando y obtener el JsonResult
        //        using (var reader = await cmd.ExecuteReaderAsync())
        //        {
        //            string jsonResult = "[]"; // Valor predeterminado si no hay datos

        //            if (reader.HasRows)
        //            {
        //                // Leer el JsonResult devuelto por el procedimiento almacenado
        //                await reader.ReadAsync();
        //                jsonResult = reader[0].ToString() ?? "[]"; // Recuperar el primer campo como JSON
        //            }

        //            // Cerrar el DataReader para liberar los parámetros de salida
        //            reader.Close();

        //            // Obtener los valores de los parámetros de salida
        //            int retorno = (int)retornoParam.Value;
        //            string mensaje = mensajeParam.Value?.ToString();

        //            // Verificar si hubo un error en el procedimiento almacenado
        //            if (retorno != 0)
        //            {
        //                return StatusCode(500, $"Error: {mensaje}");
        //            }

        //            // Deserializar el JSON completo
        //            var muestras = JsonConvert.DeserializeObject<List<MuestraRecolector>>(jsonResult);

        //            // Devolver una respuesta con los datos y los parámetros de salida
        //            return Ok(muestras);
        //            //return Ok(new
        //            //{
        //            //    Retorno = retorno,
        //            //    Mensaje = mensaje,
        //            //    Datos = muestras
        //            //});
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Manejo de errores
        //        return StatusCode(500, $"Error al ejecutar el procedimiento almacenado: {ex.Message}");
        //    }
        //    finally
        //    {
        //        // Asegurarse de cerrar la conexión
        //        if (context.Database.GetDbConnection().State == ConnectionState.Open)
        //        {
        //            await context.Database.CloseConnectionAsync();
        //        }
        //    }
        //}

        private static (int day, int month, int year) GetCurrentDate()
        {
            DateTime now = DateTime.Now;
            return (now.Day, now.Month, now.Year);
        }

        private static bool IsValidRequestDay(int day)
        {
            return day == 1 || day == 15;
        }

        private static (int month, int year) GetPreviousMonthAndYear(int month, int year)
        {
            if (month == 1)
            {
                return (12, year - 1);
            }
            return (month - 1, year);
        }

        [HttpGet("MuestraLinq/{empleado}")]
        public IActionResult MuestrasFiltradas([FromRoute] string empleado)
        {
            try
            {
                // Validar que el parámetro "empleado" no esté vacío
                if (string.IsNullOrEmpty(empleado))
                {
                    return BadRequest("El parámetro 'empleado' es obligatorio.");
                }

                // Obtener el día, mes y año actuales
                var (currentDay, currentMonth, currentYear) = GetCurrentDate();

                //var previousmonth = currentMonth - 1;

                // Crear la variable como cadena concatenada:
                string variable = (currentDay < 15 ? "1" : "2") + currentMonth.ToString() + currentYear.ToString();

                // Construir la consulta LINQ
                var query = (from ei in context.Informantes //context.EnumeradorInformante
                             join m in context.CampoMuestrasSeriePrecios on ei.CodInformante equals m.InformanteId
                             join l in context.LoginUsuarios on ei.IdEmpleado equals l.IdEmpleado
                             where ei.Activo && l.Usuario == empleado
                             //&& m.Fecha >= startDate && m.Fecha <= endDate
                             && m.muestraid == variable
                             && (
                                 //(m.Semana == 1 || m.Semana == 2)
                                 (currentDay < 15 && (m.Semana == 1 || m.Semana == 2)) ||
                                 (currentDay >= 15 && (m.Semana == 3 || m.Semana == 4))
                             )
                             select new
                             {
                                 m.InformanteId,
                                 m.VariedadId,
                                 m.Descripcion,
                                 m.Especificacion,
                                 m.Detalle,
                                 m.Fecha,
                                 m.Anio,
                                 m.Mes,
                                 m.muestraid,
                                 m.Semana,
                                 m.DiaSemanaId,
                                 m.Nveces,
                                 m.EsPesable,
                                 m.PrecioRecolectadoAnt,
                                 m.CantidadAnt,
                                 m.UnidadMedidaId,
                                 m.ObservacionAnalista,
                                 m.MonedaId,
                                 m.PesoAnt,
                                 m.PrecioCalculado
                             }).ToList(); // Materializar la consulta

                // Establecer encabezados para evitar caché
                Response.Headers["Cache-Control"] = "no-store";
                Response.Headers["Pragma"] = "no-cache";

                // Devolver la respuesta con los resultados
                return Ok(query);

                //// Filtrar CampoMuestrasSeriePrecios por muestraid
                //var muestras = (from ei in context.EnumeradorInformante
                //                join m in context.CampoMuestrasSeriePrecios
                //                on ei.CodInformante equals m.InformanteId
                //                where ei.Activo && ei.IdEmpleado == empleado && m.muestraid == variable
                //                && (
                //                   (m.Semana == 1 || m.Semana == 2)
                //                //    (currentDay < 15 && (m.Semana == 1 || m.Semana == 2)) ||
                //                //    (currentDay >= 15 && (m.Semana == 3 || m.Semana == 4))
                //                )
                //                select new
                //                {
                //                    m.InformanteId,
                //                    m.VariedadId,
                //                    m.Fecha
                //                }).ToList();

                //if (muestras.Count > 0)
                //{
                //    // Verificar que todas las llaves están en ambas tablas
                //    var informanteIds = muestras.Select(m => m.InformanteId).Distinct().ToList();
                //    var variedadIds = muestras.Select(m => m.VariedadId).Distinct().ToList();
                //    var fechas = muestras.Select(m => m.Fecha.Date).Distinct().ToList();
                //    //var fechas = muestras.Select(m => m.Fecha).Where(f => f.HasValue).Select(f => f.Value.Date).Distinct().ToList(); // Filtrar fechas nulas y obtener solo la parte de la fecha

                //    // Validar que todos los registros de muestras están en Seriesprecios
                //    var seriesPreciosExistentes = context.SeriesPrecios
                //        .Where(sp => informanteIds.Contains(sp.InformanteId)
                //                     && variedadIds.Contains(sp.VariedadId)
                //                    && fechas.Contains(sp.Fecha.Date))
                //        .Select(sp => new { sp.InformanteId, sp.VariedadId, sp.Fecha })
                //        .ToList();

                //    if (seriesPreciosExistentes.Count != muestras.Count)
                //    {
                //        return Unauthorized("Faltan registros de muestra");
                //    }
                //    else
                //    {
                //        // Eliminar de CampoMuestrasSeriePrecio los registros con muestraid == variable
                //        var registrosAEliminar = context.CampoMuestrasSeriePrecios
                //            .Where(e => e.muestraid == variable
                //                    && informanteIds.Contains(e.InformanteId)
                //                     && variedadIds.Contains(e.VariedadId)
                //                     && fechas.Contains(e.Fecha.Date)
                //            )
                //            .ToList();
                //        if (registrosAEliminar.Any())
                //        {
                //            context.CampoMuestrasSeriePrecios.RemoveRange(registrosAEliminar);
                //            context.SaveChanges();
                //        }
                //    }
                //}

                //no va Obtener el primer día y el último día del rango según el valor de currentDay
                //DateTime startDate;
                //DateTime endDate;

                //if (currentDay < 15)
                //{
                //    // Rango: Del 1 al 14 del mes actual
                //    startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                //    endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 15);
                //}
                //else
                //{
                //    // Rango: Del 15 al último día del mes actual
                //    startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 15);
                //    endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));
                //}
            }
            catch (Exception ex)
            {
                // Manejar errores y devolver una respuesta de error
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        //[HttpGet("MuestraLinqD/{empleado}")]
        //public IActionResult GetFilteredMuestras2([FromRoute] string empleado)
        //{
        //    try
        //    {
        //        // Validar que el parámetro "empleado" no esté vacío
        //        if (string.IsNullOrEmpty(empleado))
        //        {
        //            return BadRequest("El parámetro 'empleado' es obligatorio.");
        //        }

        //        // Obtener el día actual del mes
        //        int currentDay = DateTime.Now.Day;

        //        // Construir la consulta LINQ
        //        var query = (from ei in context.EnumeradorInformante
        //                     join m in context.Muestras
        //                     on ei.CodInformante equals m.InformanteId
        //                     where ei.Activo && m.EsProductoActivo && ei.IdEmpleado == empleado
        //                           && (
        //                               (currentDay <= 14 && (m.Sem1 || m.Sem2)) ||
        //                               (currentDay >= 15 && (m.Sem3 || m.Sem4))
        //                           )
        //                     select new
        //                     {
        //                         m.InformanteId,
        //                         m.VariedadId,
        //                         m.Detalle,
        //                         m.Sem1,
        //                         m.Sem2,
        //                         m.Sem3,
        //                         m.Sem4,
        //                         m.DiaSemanaId,
        //                         m.Nveces,
        //                         Usuario = "", // Campo vacío para usuario
        //                         Lng = "",     // Campo vacío para lng
        //                         Lat = ""      // Campo vacío para lat
        //                     }).ToList(); // Materializar la consulta

        //        // Devolver la respuesta con los resultados
        //        return Ok(query);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Manejar errores y devolver una respuesta de error
        //        return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        //    }
        //}

        [HttpGet("Catalogos/{empleado}")]
        public async Task<IActionResult> InformanteVariedad([FromRoute] string empleado)
        {
            // Validar que el parámetro "empleado" no esté vacío
            if (string.IsNullOrEmpty(empleado))
            {
                return BadRequest("El parámetro 'empleado' es obligatorio.");
            }

            try
            {
                // Obtener el día, mes y año actuales
                var (currentDay, currentMonth, currentYear) = GetCurrentDate();

                // Crear la variable como cadena concatenada:
                string variable = (currentDay < 15 ? "1" : "2") + currentMonth.ToString() + currentYear.ToString();

                // Construir Informante
                var informante = await (from ei in context.Informantes //context.EnumeradorInformante
                                        join m in context.CampoMuestrasSeriePrecios on ei.CodInformante equals m.InformanteId
                                        join i in context.Informantes on m.InformanteId equals i.CodInformante
                                        join l in context.LoginUsuarios on ei.IdEmpleado equals l.IdEmpleado
                                        where ei.Activo && i.Activo && l.Usuario == empleado
                                        //&& m.Fecha >= startDate && m.Fecha <= endDate
                                        && m.muestraid == variable
                                        && (
                                            (currentDay < 15 && (m.Semana == 1 || m.Semana == 2)) ||
                                            (currentDay >= 15 && (m.Semana == 3 || m.Semana == 4))
                                          )
                                        select new InformanteListaDto // Crear instancias de la clase Informantes
                                        {
                                            CodInformante = i.CodInformante,
                                            NombreInformante = i.NombreInformante + " " + i.Direccion,
                                            Activo = i.Activo,
                                            Semana = m.Semana,
                                            Dia = m.DiaSemanaId// Asignar la semana como cadena
                                        }).Distinct().OrderBy(e => e.NombreInformante).ToListAsync(); // Materializar la consulta

                // Agrupar VariedadSemana por Informante y Semana
                var cte = from v in context.VariedadSemana
                          join ei in context.Informantes on v.Informante equals ei.CodInformante //context.EnumeradorInformante
                          join lu in context.LoginUsuarios on ei.IdEmpleado equals lu.IdEmpleado
                          where v.Activo && v.Valor && lu.Usuario == empleado
                           && (
                                (currentDay < 15 && (v.semana == "1" || v.semana == "2")) ||
                                (currentDay >= 15 && (v.semana == "3" || v.semana == "4"))
                            )
                          group v by new { v.Informante, v.semana, v.Dia } into g
                          select new CteResultado
                          {
                              Informante = g.Key.Informante,
                              Semana = g.Key.semana,
                              Dia = g.Key.Dia,
                              ConteoProductos = g.Count()

                          };
                // Unir Informantes con VariedadSemana agrupada
                var informantesemana = await (from i in context.Informantes
                                              join m in cte on i.CodInformante equals m.Informante
                                              join r in context.RegionDistrito on i.DistritoId equals r.RegionDistritoId
                                              where i.Activo
                                              select new InformanteDto
                                              {
                                                  CodInformante = i.CodInformante,
                                                  //NombreInformante = i.NombreInformante,
                                                  Direccion = TrimAndReplace(i.Direccion),
                                                  Barrio = TrimAndReplace(
                                                      i.Direccion.IndexOf("||") >= 0
                                                      ? i.Direccion.Substring(0, i.Direccion.IndexOf("||"))
                                                      : string.Empty // O cualquier valor predeterminado que desees
                                                  ),
                                                  NomRegionDistrito = r.NomRegionDistrito,
                                                  ConteoProductos = m.ConteoProductos,
                                                  Semana = m.Semana,
                                                  Dia = m.Dia
                                              }).ToListAsync();

                // Construir Variedad            
                var variedad = await (from ei in context.Informantes //context.EnumeradorInformante
                                      join m in context.CampoMuestrasSeriePrecios on ei.CodInformante equals m.InformanteId
                                      join v in context.Variedades on m.VariedadId equals v.Id
                                      join l in context.LoginUsuarios on ei.IdEmpleado equals l.IdEmpleado
                                      where ei.Activo && l.Usuario == empleado
                                       && m.muestraid == variable
                                       && (
                                            (currentDay < 15 && (m.Semana == 1 || m.Semana == 2)) ||
                                            (currentDay >= 15 && (m.Semana == 3 || m.Semana == 4))
                                          )
                                      select new VariedadesDTO // Crear instancias de la clase Variedades
                                      {
                                          Id = v.Id,
                                          Descripcion = v.Descripcion,
                                          InformanteId = m.InformanteId,
                                          Semana = m.Semana,
                                          Dia = m.DiaSemanaId
                                      }).Distinct().OrderBy(e => e.Id).ThenBy(e => e.InformanteId).ToListAsync();

                var dias = await context.DiasSemana.OrderBy(e => e.Orden).ToListAsync();

                var umedp = await (from um in context.UmedP
                                   join m in context.CampoMuestrasSeriePrecios on um.Codproducto equals m.VariedadId
                                   join eu in context.Informantes on m.InformanteId equals eu.CodInformante //context.EnumeradorInformante
                                   join l in context.LoginUsuarios on eu.IdEmpleado equals l.IdEmpleado
                                   where eu.Activo
                                         && l.Usuario == empleado
                                         && m.muestraid == variable
                                         && (
                                            (currentDay < 15 && (m.Semana == 1 || m.Semana == 2)) ||
                                            (currentDay >= 15 && (m.Semana == 3 || m.Semana == 4))
                                          )
                                   group new { um, m } by new { um.Codproducto, um.Urecol } into g
                                   select new UmedP
                                   {
                                       Codproducto = g.Key.Codproducto,
                                       Urecol = g.Key.Urecol
                                   }).OrderBy(e => e.Codproducto).ToListAsync();

                // Crear la lista de semanas dependiendo del currentDay
                List<Semana> semana = new List<Semana>();
                if (currentDay > 14)
                {
                    semana.Add(new Semana { id = 3, descripcion = "Semana 3" });
                    semana.Add(new Semana { id = 4, descripcion = "Semana 4" });
                }
                else
                {
                    semana.Add(new Semana { id = 1, descripcion = "Semana 1" });
                    semana.Add(new Semana { id = 2, descripcion = "Semana 2" });
                }

                // Crear el objeto de respuesta
                var response = new Catalogos
                {
                    Informantes = informante, // Asignar la lista de Informantes
                    Variedades = variedad,     // Asignar la lista de Variedades
                    DiasSemana = dias,     // Asignar la lista de dias
                    UmedP = umedp,          // Asignar la lista unidades
                    Semana = semana,           // Asignar la lista de semanas
                    InformanteDto = informantesemana
                };

                // Establecer encabezados para evitar caché
                Response.Headers["Cache-Control"] = "no-store";
                Response.Headers["Pragma"] = "no-cache";

                // Devolver el resultado como JSON
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Registrar el error (puedes usar ILogger para registrar en un archivo o sistema de logs)
                Console.Error.WriteLine($"Error al obtener departamentos y municipios: {ex.Message}");

                // Retornar un mensaje de error genérico al cliente
                return StatusCode(500, "Ocurrió un error al procesar la solicitud. Por favor, inténtelo más tarde.");
            }
        }

        [HttpGet("Validamuestra/{empleado}/{semana}/{dia}")]
        public async Task<IActionResult> Validamuestra([FromRoute] string empleado, [FromRoute] int semana, [FromRoute] string dia)
        {
            // Obtener el día, mes y año actuales
            var (currentDay, currentMonth, currentYear) = GetCurrentDate();

            //// Crear la variable como cadena concatenada:
            string variable = (currentDay < 15 ? "1" : "2") + currentMonth.ToString() + currentYear.ToString();

            var querys = await (from cs in context.CampoMuestrasSeriePrecios
                                join ei in context.Informantes on cs.InformanteId equals ei.CodInformante //context.EnumeradorInformante
                                join l in context.LoginUsuarios on ei.IdEmpleado equals l.IdEmpleado
                                join i in context.Informantes on cs.InformanteId equals i.CodInformante
                                where ei.Activo && l.Usuario == empleado
                                      && cs.muestraid == variable
                                      && cs.Semana == semana
                                      && cs.DiaSemanaId == dia
                                && !context.SeriesPrecios.Any(sp =>
                                     sp.InformanteId == cs.InformanteId &&
                                     sp.VariedadId == cs.VariedadId &&
                                     sp.Anio == cs.Anio &&
                                     sp.Mes == cs.Mes &&
                                     sp.Semana == cs.Semana)
                                select new
                                {
                                    i.CodInformante,
                                    i.NombreInformante,
                                    cs.Semana,
                                    cs.DiaSemanaId
                                }).Distinct().ToListAsync();

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

        [HttpGet("Validamuestraanterior/{empleado}")]
        public async Task<IActionResult> Validamuestraanterior([FromRoute] string empleado)
        {
            // Obtener el día, mes y año actuales
            var (currentDay, currentMonth, currentYear) = GetCurrentDate();

            var (previousMonth, previousYear) = GetPreviousMonthAndYear(currentMonth, currentYear);

            //// Crear la variable como cadena concatenada:
            string variable = (currentDay < 15 ? "1" : "2") + previousMonth.ToString() + previousYear.ToString();

            var querys = await (from cs in context.CampoMuestrasSeriePrecios
                                join ei in context.Informantes on cs.InformanteId equals ei.CodInformante //context.EnumeradorInformante
                                join l in context.LoginUsuarios on ei.IdEmpleado equals l.IdEmpleado
                                join i in context.Informantes on cs.InformanteId equals i.CodInformante
                                where ei.Activo && l.Usuario == empleado
                                      && cs.muestraid == variable
                                //&& cs.Semana == semana
                                //&& cs.DiaSemanaId == dia
                                && !context.SeriesPrecios.Any(sp =>
                                     sp.InformanteId == cs.InformanteId &&
                                     sp.VariedadId == cs.VariedadId &&
                                     sp.Fecha == cs.Fecha)
                                select new
                                {
                                    i.CodInformante,
                                    i.NombreInformante,
                                    cs.Semana,
                                    cs.DiaSemanaId
                                    //cs.Descripcion .Take(10)
                                }).Distinct().ToListAsync();

            // Establecer encabezados para evitar caché
            Response.Headers["Cache-Control"] = "no-store";
            Response.Headers["Pragma"] = "no-cache";

            if (querys.Count > 0)
            {
                return Ok(querys);
            }

            return Ok(querys);
            //return NotFound(new { mensaje = "La muestra ya fue completada." });
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

            var query = await (from ei in context.Informantes // context.EnumeradorInformante
                               join m in context.CampoMuestrasSeriePrecios on ei.CodInformante equals m.InformanteId
                               join l in context.LoginUsuarios on ei.IdEmpleado equals l.IdEmpleado
                               where ei.Activo && l.Usuario == empleado
                               select new
                               {
                                   Usuario = l.Usuario,
                                   Pass = l.Pass
                               }).FirstOrDefaultAsync();

            //var query1 = (from ei in context.EnumeradorInformante
            //             join m in context.CampoMuestrasSeriePrecios on ei.CodInformante equals m.InformanteId
            //             join l in context.LoginUsuarios on ei.IdEmpleado equals l.IdEmpleado
            //             where ei.IdEmpleado == empleado && ei.Activo == true
            //             select new
            //             {
            //                 Usuario = ei.IdEmpleado,
            //                 Pass = l.Pass
            //             })
            //  .Take(1)
            //  .AsEnumerable(); // Ejecutar la consulta en memoria

            //var query2 = (from l in context.LoginUsuarios
            //              where l.Usuario == "Autoriza" && l.Activo == true
            //              select new
            //              {
            //                  Usuario = l.Usuario,
            //                  Pass = l.Pass
            //              })
            //               .AsEnumerable(); // Ejecutar la consulta en memoria

            //var query = query1.Concat(query2);

            if (query == null)
            {
                return NotFound(new { mensaje = "Acceso denegado. No es un usuario activo o no tiene muestra asignada en el período." });
            }

            // Obtener el mes y año anterior
            var (previousMonth, previousYear) = GetPreviousMonthAndYear(currentMonth, currentYear);

            //// Crear la variable como cadena concatenada:
            string variable = (currentDay < 15 ? "1" : "2") + previousMonth.ToString() + previousYear.ToString();

            var querys = await (from cs in context.CampoMuestrasSeriePrecios
                                join ei in context.Informantes //context.EnumeradorInformante
                                    on cs.InformanteId equals ei.CodInformante
                                where ei.Activo && ei.IdEmpleado == empleado
                                      && cs.muestraid == variable
                                      && !context.SeriesPrecios.Any(sp =>
                                           sp.InformanteId == cs.InformanteId &&
                                           sp.VariedadId == cs.VariedadId &&
                                           sp.Fecha == cs.Fecha)
                                select new
                                {
                                    cs.InformanteId,
                                    cs.VariedadId,
                                    cs.Fecha
                                }).ToListAsync();

            if (querys.Count > 0)
            {
                return Unauthorized(new { mensaje = "Acceso denegado. La muestra del período anterior no se ha completado." });
            }
            //else
            //{
            //    // Eliminar de CampoMuestrasSeriePrecio los registros con muestraid == variable
            //    var registrosAEliminar = (from cs in context.CampoMuestrasSeriePrecios
            //                              join ei in context.EnumeradorInformante
            //                                  on cs.InformanteId equals ei.CodInformante
            //                              where ei.Activo && ei.IdEmpleado == empleado
            //                                    && cs.muestraid == "142025" //variable
            //                              select cs).ToList();

            //    //var registrosAEliminar = context.CampoMuestrasSeriePrecios
            //    //    .Where(e => e.muestraid == variable).ToList();
            //    if (registrosAEliminar.Any())
            //    {
            //        context.CampoMuestrasSeriePrecios.RemoveRange(registrosAEliminar);
            //        context.SaveChanges();
            //    }
            //}

            // Establecer encabezados para evitar caché
            Response.Headers["Cache-Control"] = "no-store";
            Response.Headers["Pragma"] = "no-cache";

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

            var query = await (from ei in context.LoginUsuarios
                               where ei.Activo && ei.Usuario == usuario && ei.Pass == clave
                               select new
                               {
                                   ei.Usuario
                               }).FirstOrDefaultAsync();

            if (query == null)
            {
                return NotFound(new { mensaje = "Acceso denegado. Credenciales Invalidas." });
            }

            //var querys = (from ei in context.EnumeradorInformante
            //              join m in context.CampoMuestrasSeriePrecios on ei.CodInformante equals m.InformanteId
            //              join l in context.LoginUsuarios on ei.IdEmpleado equals l.IdEmpleado
            //              where ei.IdEmpleado == empleado && ei.Activo
            //              select new
            //              {
            //                  Usuario = ei.IdEmpleado,
            //                  Pass = l.Pass
            //              })
            // .Take(1)
            // .Concat(
            //     from l in context.LoginUsuarios
            //     where l.Usuario == "Autoriza" && l.Activo
            //     select new
            //     {
            //         Usuario = l.Usuario,
            //         Pass = l.Pass
            //     }
            // );

            //var query1 = (from ei in context.EnumeradorInformante
            //              join m in context.CampoMuestrasSeriePrecios on ei.CodInformante equals m.InformanteId
            //              join l in context.LoginUsuarios on ei.IdEmpleado equals l.IdEmpleado
            //              where ei.IdEmpleado == empleado && ei.Activo == true
            //              select new
            //              {
            //                  Usuario = ei.IdEmpleado,
            //                  Pass = l.Pass
            //              })
            //  .Take(1)
            //  .AsEnumerable(); // Ejecutar la consulta en memoria

            //var query2 = (from l in context.LoginUsuarios
            //              where l.Usuario == "Autoriza" && l.Activo == true
            //              select new
            //              {
            //                  Usuario = l.Usuario,
            //                  Pass = l.Pass
            //              })
            //               .AsEnumerable(); // Ejecutar la consulta en memoria

            //var result = query1.Concat(query2);

            var result = await (from ei in context.Informantes // context.EnumeradorInformante
                                join m in context.CampoMuestrasSeriePrecios on ei.CodInformante equals m.InformanteId
                                join l in context.LoginUsuarios on ei.IdEmpleado equals l.IdEmpleado
                                where ei.Activo && l.Usuario == empleado
                                select new
                                {
                                    Usuario = l.Usuario,
                                    Pass = l.Pass
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

        [HttpPost("bulk")]
        public async Task<ActionResult<IEnumerable<SeriesPrecios>>> PostSeriesPrecios(IEnumerable<SeriesPrecios> seriesprecios)
        {
            if (seriesprecios == null || !seriesprecios.Any())
            {
                return BadRequest("La lista de series precios no puede estar vacía.");
            }

            // Agregar todos los productos al contexto
            context.SeriesPrecios.AddRange(seriesprecios);

            // Guardar los cambios en la base de datos
            await context.SaveChangesAsync();

            // Retornar la lista de productos creados
            return CreatedAtAction(nameof(PreciosSeries), seriesprecios);
        }

        private static bool IsNullOrEmpty<T>(IEnumerable<T>? collection) =>
            collection == null || !collection.Any();

        private static string TrimAndReplace(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "SD";
            var replaced = input.Replace("|", "");
            return replaced.Trim();
        }

        [HttpPost("bulksupin")]
        public async Task<ActionResult> PostupinSeriesPreciosMuestras(BulkInsSPUpMDto bulkInsertDto)
        {
            if (bulkInsertDto == null ||
                IsNullOrEmpty(bulkInsertDto.SeriesPrecios) &&
                IsNullOrEmpty(bulkInsertDto.Muestras) &&
                IsNullOrEmpty(bulkInsertDto.Informantes))
            {
                return BadRequest("Las listas de Series Precios, Muestras e Informantes no pueden estar vacías.");
            }

            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                //1.Insertar SeriesPrecios
                //if (!IsNullOrEmpty(bulkInsertDto.SeriesPrecios))
                //{
                //    await context.AddRangeAsync(bulkInsertDto.SeriesPrecios);
                //    await context.SaveChangesAsync();
                //}

                if (!IsNullOrEmpty(bulkInsertDto.SeriesPrecios))
                {
                    foreach (var item in bulkInsertDto.SeriesPrecios)
                    {
                        // Validar que exista la combinación (InformanteId, VariedadId) en Muestras
                        var muestraExistente = await context.Muestras
                            .FirstOrDefaultAsync(m => m.InformanteId == item.InformanteId && m.VariedadId == item.VariedadId);

                        if (muestraExistente == null)
                        {
                            // Opción: Devolver error o crear automáticamente la muestra
                            return BadRequest($"No existe una muestra con InformanteId={item.InformanteId}, VariedadId={item.VariedadId}");

                            // Alternativa: Crear automáticamente
                            // await context.AddAsync(new Muestras
                            // {
                            //     InformanteId = item.InformanteId,
                            //     VariedadId = item.VariedadId,
                            //     Nveces = 1
                            // });
                        }

                        var existing = await context.SeriesPrecios
                            .FindAsync(item.InformanteId, item.VariedadId, item.Anio, item.Mes, item.Semana);

                        //if (existing != null)
                        //{
                        //    // Actualizar las propiedades necesarias
                        //    context.Entry(existing).CurrentValues.SetValues(item);
                        //}
                        if (existing != null)
                        {
                            existing.Fecha = item.Fecha;
                            existing.PrecioRecolectado = item.PrecioRecolectado;
                            existing.Peso = item.Peso;
                            existing.Cantidad = item.Cantidad;
                            existing.UnidadMedidaId = item.UnidadMedidaId;
                            existing.EsOferta = item.EsOferta;
                            existing.TieneDescuento = item.TieneDescuento;
                            existing.Descuento = item.Descuento;
                            existing.TieneIVA = item.TieneIVA;
                            existing.TienePropina = item.TienePropina;
                            existing.MonedaId = item.MonedaId;
                            existing.EstadoProductoId = item.EstadoProductoId;
                            existing.PrecioSustituidoR = item.PrecioSustituidoR;
                            existing.PrecioSustituidoC = item.PrecioSustituidoC;
                            existing.ObservacionEnumerador = item.ObservacionEnumerador;
                            existing.FechaCreacion = item.FechaCreacion;
                            existing.CreadoPor = item.CreadoPor;
                        }
                        else
                        {
                            await context.AddAsync(item);
                        }
                    }

                    await context.SaveChangesAsync();
                }

                // 2. Actualizar Muestras
                if (!IsNullOrEmpty(bulkInsertDto.Muestras))
                {
                    foreach (var muestra in bulkInsertDto.Muestras)
                    {
                        var existing = await context.Muestras
                            .FindAsync(muestra.InformanteId, muestra.VariedadId);

                        if (existing != null)
                        {
                            existing.Nveces = muestra.Nveces;
                        }
                    }
                    await context.SaveChangesAsync();
                }

                // 3. Actualizar Informantes
                //if (!IsNullOrEmpty(bulkInsertDto.Informantes))
                //{
                //    foreach (var informante in bulkInsertDto.Informantes)
                //    {
                //        var existing = await context.Informantes
                //            .FindAsync(informante.CodInformante);

                //        if (existing != null)
                //        {
                //            existing.CoordenadaX = informante.CoordenadaX;
                //            existing.CoordenadaY = informante.CoordenadaY;
                //        }
                //    }
                //    await context.SaveChangesAsync();
                //}

                if (!IsNullOrEmpty(bulkInsertDto.Informantes))
                {
                    foreach (var item in bulkInsertDto.Informantes)
                    {
                        var existing = await context.CampoInformantes
                            .FindAsync(item.CodInformante, item.Anio, item.Mes, item.Semana);

                        if (existing != null)
                        {
                            // Actualizar las propiedades necesarias
                            //            context.Entry(existing).CurrentValues.SetValues(item);
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

    }
}
