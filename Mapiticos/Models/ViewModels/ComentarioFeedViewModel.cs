
namespace Mapiticos.Models.ViewModels
{
    public class ComentarioFeedViewModel
    {
        public int ComentarioId { get; set; }
        public int AventuraId { get; set; }
        public string NombreUsuario { get; set; } = "";
        public string Texto { get; set; } = "";
        public DateTime Fecha { get; set; }
    }
}
