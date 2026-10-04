namespace Mapiticos.Models.ViewModels
{
    // Una etiqueta que se muestra encima de la foto (Hiking, Café, Vista...)
    public class EtiquetaViewModel
    {
        public int EtiquetaId { get; set; }   // para filtrar el mapa y guardar etiquetas
        public string Nombre { get; set; } = "";
        public string Icono { get; set; } = "";   // Clase de Tabler, ej: "ti-mountain"
    }

    // Todo lo que la vista necesita para dibujar UNA publicación del feed
    public class PublicacionFeedViewModel
    {
        public int AventuraId { get; set; }
        public string NombreUsuario { get; set; } = "";
        public string? AvatarUrl { get; set; }
        public string Lugar { get; set; } = "";
        public string? FotoUrl { get; set; }
        public string Descripcion { get; set; } = "";
        public int CantidadLikes { get; set; }
        public bool LeDiLike { get; set; }
        public DateTime FechaPublicacion { get; set; }
        public List<EtiquetaViewModel> Etiquetas { get; set; } = new();

        // Primera letra del usuario, para el avatar cuando no tiene foto
        public string Inicial =>
            string.IsNullOrEmpty(NombreUsuario) ? "?" : NombreUsuario[..1].ToUpper();

        // Texto tipo "Hace 2 horas"
        public string TiempoTranscurrido
        {
            get
            {
                var diferencia = DateTime.Now - FechaPublicacion;
                if (diferencia.TotalMinutes < 60) return $"Hace {(int)diferencia.TotalMinutes} min";
                if (diferencia.TotalHours < 24) return $"Hace {(int)diferencia.TotalHours} h";
                if (diferencia.TotalDays < 2) return "Ayer";
                return $"Hace {(int)diferencia.TotalDays} días";
            }
        }
    }
}