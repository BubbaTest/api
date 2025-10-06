using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Alexa.DAL;
using Alexa.DAL.Seguridad;
using Alexa.DAL.IPC;
using Alexa.DAL.IPP;
using Alexa.DAL.Certificado;

namespace Alexa
{
    public class SecondaryDbContext : DbContext
    {
        public SecondaryDbContext(DbContextOptions<SecondaryDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Rol> Rol { get; set; }
        public DbSet<relUsuarioRol> relUsuarioRol { get; set; }
        public DbSet<tblcertificados> tblcertificados { get; set; }
    }

    public class EinkommenDbContext : DbContext
    {
        public EinkommenDbContext(DbContextOptions<EinkommenDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<SessionLog> SessionLogs { get; set; }
        public DbSet<relUsuarioRol> relUsuarioRol { get; set; }

    }

    public class CenagroDbContext : DbContext
    {
        public CenagroDbContext(DbContextOptions<CenagroDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<SessionLog> SessionLogs { get; set; }
        public DbSet<relUsuarioRol> relUsuarioRol { get; set; }
        public DbSet<Certificados> Certificados { get; set; }

    }

    public class SisanomDbContext : DbContext
    {
        public SisanomDbContext(DbContextOptions<SisanomDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<SessionLog> SessionLogs { get; set; }
        public DbSet<relUsuarioRol> relUsuarioRol { get; set; }

    }

    public class CapacitacionDbContext : DbContext
    {
        public CapacitacionDbContext(DbContextOptions<CapacitacionDbContext> options) : base(options)
        {
        }

        public DbSet<user> users { get; set; }
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<SessionLog> SessionLogs { get; set; }
        public DbSet<relUsuarioRol> relUsuarioRol { get; set; }
        public DbSet<DatosPuntosMapa> DatosPuntosMapas { get; set; }

    }

    public class CatalogsDbContext : DbContext
    {
        public CatalogsDbContext(DbContextOptions<CatalogsDbContext> options) : base(options)
        {
        }
    }

    public class IpcDbContext : DbContext
    {
        public IpcDbContext(DbContextOptions<IpcDbContext> options) : base(options)
        {
        }

        public DbSet<EnumeradorInformante> EnumeradorInformante { get; set; }

        public DbSet<Muestras> Muestras { get; set; }

        public DbSet<Informantes> Informantes { get; set; }

        public DbSet<Variedades> Variedades { get; set; }

        public DbSet<DiasSemana> DiasSemana { get; set; }

        public DbSet<UmedP> UmedP { get; set; }

        public DbSet<SeriesPrecios> SeriesPrecios { get; set; }
        public DbSet<LoginUsuarios> LoginUsuarios { get; set; }

        public DbSet<CampoMuestrasSeriePrecios> CampoMuestrasSeriePrecios { get; set; }

        public DbSet<VariedadSemana> VariedadSemana { get; set; }
        public DbSet<RegionDistrito> RegionDistrito { get; set; }
        public DbSet<CampoInformantes> CampoInformantes { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // Configurar clave compuesta para EnumeradorInformante
            modelBuilder.Entity<EnumeradorInformante>()
                .HasKey(ei => new { ei.CodInformante, ei.IdEmpleado }); // Clave compuesta

            // Configurar clave compuesta para Muestras
            modelBuilder.Entity<Muestras>()
                .HasKey(m => new { m.InformanteId, m.VariedadId }); // Clave compuesta

            // Configurar clave compuesta para Muestras
            modelBuilder.Entity<Variedades>()
                .HasKey(m => new { m.Id, m.InformanteId }); // Clave compuesta

            // Configurar clave compuesta para Umedo
            modelBuilder.Entity<UmedP>()
                .HasKey(m => new { m.Codproducto, m.Urecol }); // Clave compuesta

            // Configurar clave compuesta para Umedo
            modelBuilder.Entity<CampoMuestrasSeriePrecios>()
                .HasKey(m => new { m.InformanteId, m.VariedadId, m.Fecha }); // Clave compuesta

            modelBuilder.Entity<SeriesPrecios>()
               .HasKey(m => new { m.InformanteId, m.VariedadId, m.Anio, m.Mes, m.Semana }); // Clave compuesta

            modelBuilder.Entity<VariedadSemana>()
              .HasKey(m => new { m.Informante, m.Variedad, m.semana }); // Clave compuesta

            modelBuilder.Entity<CampoInformantes>()
              .HasKey(m => new { m.CodInformante, m.Anio, m.Mes, m.Semana }); // Clave compuesta

            //modelBuilder.Entity<Informantes>()
            //  .HasKey(m => new { m.CodInformante, m.IdEmpleado }); // Clave compuesta


            // Asignar la entidad Muestras al esquema Ipc
            modelBuilder.Entity<Muestras>().ToTable("Muestras", "Ipc");
            modelBuilder.Entity<Informantes>().ToTable("Informantes", "Ipc");
            modelBuilder.Entity<Variedades>().ToTable("Variedades", "Ipc");
            modelBuilder.Entity<SeriesPrecios>().ToTable("SeriesPrecios", "Ipc");
            modelBuilder.Entity<CampoMuestrasSeriePrecios>().ToTable("CampoMuestrasSeriePrecios", "Ipc");
            modelBuilder.Entity<CampoInformantes>().ToTable("CampoInformantes", "Ipc");
        }
    }

    public class IppDbContext : DbContext
    {
        public IppDbContext(DbContextOptions<IppDbContext> options) : base(options)
        {
        }

        public DbSet<CatCatalogo> CatCatalogo { get; set; }
        public DbSet<AsignacionPersonal> AsignacionPersonal { get; set; }
        public DbSet<CatCalendario> CatCalendario { get; set; }
        public DbSet<CatCanasta> CatCanasta { get; set; }
        public DbSet<CatEstablecimiento> CatEstablecimiento { get; set; }
        public DbSet<CatTipoCambio> CatTipoCambio { get; set; }
        public DbSet<CatUMedVar> CatUMedVar { get; set; }
        public DbSet<CatUnidadMedida> CatUnidadMedida { get; set; }
        public DbSet<CatValorCatalogo> CatValorCatalogo { get; set; }
        public DbSet<CatVariedad> CatVariedad { get; set; }
        public DbSet<Detalle> Detalle { get; set; }
        public DbSet<EstablecimientoCanasta> EstablecimientoCanasta { get; set; }
        public DbSet<Muestra> Muestra { get; set; }
        public DbSet<SEC_EMPLEADO> SEC_EMPLEADO { get; set; }
        public DbSet<SEC_MUNI> SEC_MUNI { get; set; }
        public DbSet<AsignarZona> AsignarZona { get; set; }
        public DbSet<UsuarioIPP> Usuario { get; set; }
        public DbSet<SessionLog> SessionLogs { get; set; }
        public DbSet<relUsuarioRol> relUsuarioRol { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // Configurar clave compuesta para EnumeradorInformante
            modelBuilder.Entity<Detalle>()
                .HasKey(ei => new { ei.ObjIdEstablecimientoCanasta, ei.ObjIdCatVariedad, ei.FechaDefinidaRecoleccion }); // Clave compuesta

            modelBuilder.Entity<EstablecimientoCanasta>()
                .HasOne(ec => ec.CatEstablecimiento)
                .WithMany()
                .HasForeignKey(ec => ec.ObjIdCatEstablecimiento)
                .OnDelete(DeleteBehavior.NoAction);  // Opciones: NoAction, Restrict, SetNull

            // Asignar la entidad Muestras al esquema Ipc
            modelBuilder.Entity<Detalle>().ToTable("Detalle", "dbo");
            modelBuilder.Entity<UsuarioIPP>().ToTable("Usuario", "sde");
        }
    }

    public class IppDeskDbContext : DbContext
    {
        public IppDeskDbContext(DbContextOptions<IppDeskDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<SessionLog> SessionLogs { get; set; }
        public DbSet<relUsuarioRol> relUsuarioRol { get; set; }


    }

    public class ArtemisaDbContext : DbContext
    {
        public ArtemisaDbContext(DbContextOptions<ArtemisaDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<SessionLog> SessionLogs { get; set; }
        public DbSet<relUsuarioRol> relUsuarioRol { get; set; }

    }
}
