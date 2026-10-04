namespace Mapiticos.Models.ViewModels
{
    // Pantalla "Ver lugar": el lugar + las aventuras que el usuario puede ver
    public class LugarDetalleViewModel
    {
        public int LugarId { get; set; }
        public string Nombre { get; set; } = "";
        public string Provincia { get; set; } = "";
        public decimal Latitud { get; set; }
        public decimal Longitud { get; set; }

        public bool EsFavorito { get; set; }
        public int CantidadFavoritos { get; set; }

        public string Filtro { get; set; } = "todas";   // "todas" o "amigos" (la pestaña activa)

        public List<EtiquetaViewModel> Etiquetas { get; set; } = new();          // las fijas del lugar
        public List<AventuraLugarViewModel> Aventuras { get; set; } = new();
    }

    // Una aventura (publicación de una persona) dentro del lugar
    public class AventuraLugarViewModel
    {
        public int AventuraId { get; set; }
        public string? Descripcion { get; set; }
        public DateTime FechaPublicacion { get; set; }

        public string NombreUsuario { get; set; } = "";
        public string NombreMostrar { get; set; } = "";
        public string? AvatarUrl { get; set; }
        public bool EsMia { get; set; }

        public int CantidadLikes { get; set; }
        public bool MeGusta { get; set; }
        public int CantidadComentarios { get; set; }

        public List<FotoViewModel> Fotos { get; set; } = new();
        public List<EtiquetaViewModel> Etiquetas { get; set; } = new();          // las personales
    }

    // Una foto de una aventura
    public class FotoViewModel
    {
        public int FotoId { get; set; }
        public string Url { get; set; } = "";
    }

    // Respuestas de los botones (se mandan como JSON)
    public class LikeResultadoViewModel
    {
        public bool MeGusta { get; set; }
        public int CantidadLikes { get; set; }
    }

    public class FavoritoResultadoViewModel
    {
        public bool EsFavorito { get; set; }
        public int CantidadFavoritos { get; set; }
    }
}