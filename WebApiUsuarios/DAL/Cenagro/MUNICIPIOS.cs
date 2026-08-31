using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.Cenagro
{
    public class MUNICIPIOS
    {        
        public string? Nom_Munici { get; set; }
        [Key]
        public string Id_Municip { get; set; } = string.Empty;
        public string? Nom_Depart { get; set; }
        public string? Id_Departa { get; set; }
        
    }
}
