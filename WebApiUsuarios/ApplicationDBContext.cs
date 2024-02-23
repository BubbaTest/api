using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Alexa.DAL.Catalogos;
using Alexa.DAL.Catalogos.UsoEdificacion;
using Alexa.DAL.Catalogos.ResidentesViviendaIdentificacionHogar;
using Alexa.DAL.Catalogos.IdentificacionUbicacionGeografica;
using Alexa.DAL.Catalogos.DatosVivienda;
using Alexa.DAL.Catalogos.CaracteristicasHogar;
using Alexa.DAL.Catalogos.CaracteristicasPersonas;
using Alexa.DAL.Catalogos.DefuncionesHogar;
using Alexa.DAL.Catalogos.ResultadoEntrevista;
using Alexa.DAL.Catalogos.CensoEconomico;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Alexa.DAL;
using Alexa.DAL.Seguridad;

namespace Alexa
{
    //public sealed class ApplicationDBContext : IdentityDbContext 
    //{
    //    public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
    //    {
    //    }

    //    protected override void OnModelCreating(ModelBuilder modelBuilder)
    //    {
    //        base.OnModelCreating(modelBuilder);

    //        modelBuilder.Entity<AutorLibro>()
    //            .HasKey(al => new { al.LibroId, al.AutorId });
    //    }

    //    public DbSet<Usuario> Usuario { get; set; }
    //    public DbSet<Rol> Rol { get; set; }
    //    public DbSet<relUsuarioRol> relUsuarioRol { get; set; }
    //    public DbSet<test> test { get; set; }

    //    public DbSet<Autor> Autor { get; set; }
    //    public DbSet<Libro> Libro { get; set; }
    //    public DbSet<Comentario> Comentario { get; set; }

    //    public DbSet<AutorLibro> AutorLibro { get; set;}
    //}

    public class SecondaryDbContext : DbContext
    {
        public SecondaryDbContext(DbContextOptions<SecondaryDbContext> options) : base(options)
        {
        }
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Rol> Rol { get; set; }
        public DbSet<relUsuarioRol> relUsuarioRol { get; set; }
    }

    public class CatalogsDbContext : DbContext
    {
        public CatalogsDbContext(DbContextOptions<CatalogsDbContext> options) : base(options)
        {
        }
       

        //General
        public DbSet<CatSiNo> CatSiNo { get; set; }

        //IdentificacionUbicacionGeografica
        public DbSet<CatS01AP02> CatS01AP02 { get; set; }
        public DbSet<CatS01AP03> CatS01AP03 { get; set; }
        public DbSet<CatS01AP06> CatS01AP06 { get; set; }
        public DbSet<CatS01AP07> CatS01AP07 { get; set; }
        public DbSet<CatS01AP08> CatS01AP08 { get; set; }
        public DbSet<CatS01AP10> CatS01AP10 { get; set; }
        public DbSet<CatS01AP11> CatS01AP11 { get; set; }
        public DbSet<CatS01AP12> CatS01AP12 { get; set; }

        //UsoEdificacion
        public DbSet<CatS01BP01> CatS01BP01 { get; set; }

        //ResidentesViviendaIdentificacionHogar
        public DbSet<CatS02P01> CatS02P01 { get; set; }

        //DatosVivienda
        public DbSet<CatS03P01> CatS03P01 { get; set; }
        public DbSet<CatS03P02> CatS03P02 { get; set; }
        public DbSet<CatS03P03> CatS03P03 { get; set; }
        public DbSet<CatS03P04> CatS03P04 { get; set; }
        public DbSet<CatS03P05> CatS03P05 { get; set; }
        public DbSet<CatS03P06> CatS03P06 { get; set; }
        public DbSet<CatS03P07> CatS03P07 { get; set; }
        public DbSet<CatS03P08> CatS03P08 { get; set; }
        public DbSet<CatS03P09> CatS03P09 { get; set; }
        public DbSet<CatS03P10_1> CatS03P10_1 { get; set; }
        public DbSet<CatS03P11_1> CatS03P11_1 { get; set; }
        public DbSet<CatS03P12_1> CatS03P12_1 { get; set; }

        //CaracteristicasHogar
        public DbSet<CatS04P02> CatS04P02 { get; set; }
        public DbSet<CatS04P03> CatS04P03 { get; set; }
        public DbSet<CatS04P04> CatS04P04 { get; set; }

        //CaracteristicasPersonas
        public DbSet<CatS06P01> CatS06P01 { get; set; }
        public DbSet<CatS06P01A> CatS06P01A { get; set; }
        public DbSet<CatS06P02> CatS06P02 { get; set; }
        public DbSet<CatS06P04> CatS06P04 { get; set; }
        public DbSet<CatS06P05> CatS06P05 { get; set; }
        public DbSet<CatS06P06> CatS06P06 { get; set; }
        public DbSet<CatS06P08> CatS06P08 { get; set; }
        public DbSet<CatS06P09> CatS06P09 { get; set; }
        public DbSet<CatS06P12_1> CatS06P12_1 { get; set; }
        public DbSet<CatS06P14> CatS06P14 { get; set; }
        public DbSet<CatS06P16> CatS06P16 { get; set; }
        public DbSet<CatS06P17_1> CatS06P17_1 { get; set; }
        public DbSet<CatS06P19_2> CatS06P19_2 { get; set; }
        public DbSet<CatS06P24> CatS06P24 { get; set; }
        public DbSet<CatS06P26> CatS06P26 { get; set; }
        public DbSet<CatS06P29> CatS06P29 { get; set; }
        public DbSet<CatS06P34> CatS06P34 { get; set; }
        public DbSet<CatS06P35> CatS06P35 { get; set; }

        //DefuncionesHogar
        public DbSet<CatS08P07> CatS08P07 { get; set; }

        //ResultadoEntrevista
        public DbSet<CatRESULTADO> CatRESULTADO { get; set; }

        //CensoEconomico
        public DbSet<CatRESULTADOEE> CatRESULTADOEE { get; set; }
        public DbSet<CatSE03P05> CatSE03P05 { get; set; }
        public DbSet<CatSE03P06> CatSE03P06 { get; set; }
        public DbSet<CatSE03P07> CatSE03P07 { get; set; }
    } 
}
