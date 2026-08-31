using Alexa.DAL;
using Alexa.DAL.Capacitacion;
using Alexa.DAL.Cenagro;
using Alexa.DAL.Certificado;
using Alexa.DAL.IPC;
using Alexa.DAL.IPP;
using Alexa.DAL.Seguridad;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace Alexa
{
    public class SecondaryDbContext(DbContextOptions<SecondaryDbContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Rol> Rol { get; set; }
        public DbSet<relUsuarioRol> relUsuarioRol { get; set; }
        public DbSet<tblcertificados> tblcertificados { get; set; }
    }

    public class EinkommenDbContext(DbContextOptions<EinkommenDbContext> options) : DbContext(options)
    {
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<SessionLog> SessionLogs { get; set; }
        public DbSet<relUsuarioRol> relUsuarioRol { get; set; }
    }

    public class CenagroDbContext(DbContextOptions<CenagroDbContext> options) : DbContext(options)
    {
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<SessionLog> SessionLogs { get; set; }
        public DbSet<relUsuarioRol> relUsuarioRol { get; set; }
        public DbSet<Certificados> Certificados { get; set; }
        public DbSet<MUNICIPIOS> MUNICIPIOS { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {   
            modelBuilder.Entity<MUNICIPIOS>().ToTable("MUNICIPIOS", "sde");
        }
    }

    public class SisanomDbContext(DbContextOptions<SisanomDbContext> options) : DbContext(options)
    {
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<SessionLog> SessionLogs { get; set; }
        public DbSet<relUsuarioRol> relUsuarioRol { get; set; }
    }

    public class CapacitacionDbContext(DbContextOptions<CapacitacionDbContext> options) : DbContext(options)
    {
        public DbSet<user> users { get; set; }
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<SessionLog> SessionLogs { get; set; }
        public DbSet<relUsuarioRol> relUsuarioRol { get; set; }
        public DbSet<DatosPuntosMapa> DatosPuntosMapas { get; set; }
    }

    public class CatalogsDbContext(DbContextOptions<CatalogsDbContext> options) : DbContext(options)
    {
    }

    public class IpcDbContext(DbContextOptions<IpcDbContext> options) : DbContext(options)
    {
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
            modelBuilder.Entity<EnumeradorInformante>().HasKey(ei => new { ei.CodInformante, ei.IdEmpleado });
            modelBuilder.Entity<Muestras>().HasKey(m => new { m.InformanteId, m.VariedadId });
            modelBuilder.Entity<Variedades>().HasKey(m => new { m.Id, m.InformanteId });
            modelBuilder.Entity<UmedP>().HasKey(m => new { m.Codproducto, m.Urecol });
            modelBuilder.Entity<CampoMuestrasSeriePrecios>().HasKey(m => new { m.InformanteId, m.VariedadId, m.Fecha });
            modelBuilder.Entity<SeriesPrecios>().HasKey(m => new { m.InformanteId, m.VariedadId, m.Anio, m.Mes, m.Semana });
            modelBuilder.Entity<VariedadSemana>().HasKey(m => new { m.Informante, m.Variedad, m.semana });
            modelBuilder.Entity<CampoInformantes>().HasKey(m => new { m.CodInformante, m.Anio, m.Mes, m.Semana });

            modelBuilder.Entity<Muestras>().ToTable("Muestras", "Ipc");
            modelBuilder.Entity<Informantes>().ToTable("Informantes", "Ipc");
            modelBuilder.Entity<Variedades>().ToTable("Variedades", "Ipc");
            modelBuilder.Entity<SeriesPrecios>().ToTable("SeriesPrecios", "Ipc");
            modelBuilder.Entity<CampoMuestrasSeriePrecios>().ToTable("CampoMuestrasSeriePrecios", "Ipc");
            modelBuilder.Entity<CampoInformantes>().ToTable("CampoInformantes", "Ipc");
        }
    }

    public class IppDbContext(DbContextOptions<IppDbContext> options) : DbContext(options)
    {
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
        public DbSet<relUsuarioUbicacionCanasta> relUsuarioUbicacionCanasta { get; set; }
        public DbSet<MuestraM> MuestraM { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder);

            // Filtro global: Cada vez que consultes 'Encuestas', 
            // EF Core agregará automáticamente "WHERE Activo = 1" en el SQL
            //modelBuilder.Entity<CatEstablecimiento>().HasQueryFilter(e => e.Activo);

            modelBuilder.Entity<Detalle>().HasKey(ei => new { ei.ObjIdEstablecimientoCanasta, ei.ObjIdCatVariedad, ei.muestraid, ei.ObjIdCatCanasta, ei.ObjCodMuni }); //ei.FechaDefinidaRecoleccion,
            modelBuilder.Entity<UsuarioIPP>().HasKey(u => u.UsuarioId);
            modelBuilder.Entity<relUsuarioUbicacionCanasta>().HasKey(ei => new { ei.UsuarioId, ei.Id_Municip, ei.IdCatCanasta });
            modelBuilder.Entity<MuestraM>().HasKey(ei => new { ei.ObjIdEstablecimientoCanasta, ei.ObjIdCatVariedad, ei.muestraid });

            modelBuilder.Entity<EstablecimientoCanasta>()
                .HasOne(ec => ec.CatEstablecimiento)
                .WithMany()
                .HasForeignKey(ec => ec.ObjIdCatEstablecimiento)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Muestra>()
                .ToTable(tb => tb.UseSqlOutputClause(false));

            modelBuilder.Entity<CatEstablecimiento>()
                .ToTable(tb => tb.UseSqlOutputClause(false));
            //.ToTable(tb => tb.UseSqlReturningClause(false));

            modelBuilder.Entity<CatTipoCambio>()
                .Property(c => c.Cambio)
                .HasPrecision(18, 14);

            modelBuilder.Entity<Detalle>().ToTable("Detalle", "dbo");
            modelBuilder.Entity<Detalle>()
                .ToTable(tb => tb.UseSqlOutputClause(false));
            modelBuilder.Entity<UsuarioIPP>().ToTable("Usuario", "sde");
            modelBuilder.Entity<relUsuarioUbicacionCanasta>().ToTable("relUsuarioUbicacionCanasta", "sde");
        }
    }

    public class IppDeskDbContext(DbContextOptions<IppDeskDbContext> options) : DbContext(options)
    {
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<SessionLog> SessionLogs { get; set; }
        public DbSet<relUsuarioRol> relUsuarioRol { get; set; }
    }

    public class ArtemisaDbContext(DbContextOptions<ArtemisaDbContext> options) : DbContext(options)
    {
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<SessionLog> SessionLogs { get; set; }
        public DbSet<relUsuarioRol> relUsuarioRol { get; set; }
    }

    public class LocalleDbContext(DbContextOptions<LocalleDbContext> options) : DbContext(options)
    {
        public DbSet<Test> Test { get; set; }
    }
}
