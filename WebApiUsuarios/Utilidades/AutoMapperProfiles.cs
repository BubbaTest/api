//using AutoMapper;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using WebApiUsuarios.DTOs;
//using WebApiUsuarios.Entidades;

//namespace WebApiUsuarios.Utilidades
//{
//    public class AutoMapperProfiles: Profile
//    {
//        public AutoMapperProfiles()
//        {
//            CreateMap<AutorCreacionDTO, Autor>();
//            CreateMap<Autor, AutorDTO>();
//            CreateMap<Autor, AutorDTOConLibros>()
//                .ForMember(autorDTOConLibros => autorDTOConLibros.Libros, opciones => opciones.MapFrom(MapAutorDTOLibros));
//            //CreateMap<Autor, AutorDTO>()
//            //    .ForMember(autorDTO => autorDTO.Libros, opciones => opciones.MapFrom(MapAutorDTOLibros)); 

//            CreateMap<LibroCreacionDTO, Libro>()
//                .ForMember(libro => libro.AutoresLibros, opciones => opciones.MapFrom(MapAutoresLibros));
//            CreateMap<Libro, LibroDTO>();
//            CreateMap<Libro, LibroDTOConAutores>()
//                .ForMember(libroDTOConAutores => libroDTOConAutores.Autores, opciones => opciones.MapFrom(MapLibroDTOAutores));
//            //CreateMap<Libro, LibroDTO>()
//            //    .ForMember(libroDTO => libroDTO.Autores, opciones => opciones.MapFrom(MapLibroDTOAutores));                
//            CreateMap<LibroPatchDTO, Libro>().ReverseMap();

//            CreateMap<ComentarioCreacionDTO, Comentario>();
//            CreateMap<Comentario, ComentarioDTO>();
//        }

//        private List<AutorLibro> MapAutoresLibros(LibroCreacionDTO libroCreacionDTO, Libro libro) 
//        {
//            var resultado = new List<AutorLibro>();
//            if (libroCreacionDTO.AutoresIds == null) { return resultado; }

//            foreach (var autorId in libroCreacionDTO.AutoresIds) 
//            {
//                resultado.Add(new AutorLibro() { AutorId = autorId });
//            }

//            return resultado;
//        }

//        private List<AutorDTO> MapLibroDTOAutores(Libro libro, LibroDTO libroDTO)
//        {
//            var resultado = new List<AutorDTO>();
//            if (libro.AutoresLibros == null) { return resultado; }

//            foreach (var autorlibro in libro.AutoresLibros)
//            {
//                resultado.Add(new AutorDTO() 
//                { 
//                    Id = autorlibro.AutorId, 
//                    Nombre = autorlibro.Autor.Nombre
//                });
//            }

//            return resultado;
//        }

//        private List<LibroDTO> MapAutorDTOLibros(Autor autor, AutorDTO autorDTO)
//        {
//            var resultado = new List<LibroDTO>();
//            if (autor.AutoresLibros == null) { return resultado; }

//            foreach (var autorlibro in autor.AutoresLibros)
//            {
//                resultado.Add(new LibroDTO()
//                {
//                    Id = autorlibro.LibroId,
//                    Titulo = autorlibro.Libro.Titulo
//                });
//            }

//            return resultado;
//        }
//    }
//}
