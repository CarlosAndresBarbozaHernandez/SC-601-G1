namespace Mapiticos.Models.ViewModels
{
    // Estado de "yo sigo a esta persona" (columna Seguimientos.Estado + "Ninguno")
    public static class EstadosSeguimiento
    {
        public const string Ninguno = "Ninguno";
        public const string Pendiente = "Pendiente";   // cuenta privada, falta que acepte
        public const string Aceptado = "Aceptado";     // ya la sigo
    }

    // Resultado del buscador de personas
    public class UsuarioBusquedaViewModel
    {
        public string NombreUsuario { get; set; } = "";
        public string NombreMostrar { get; set; } = "";
        public string? AvatarUrl { get; set; }
        public bool EsPrivada { get; set; }
        public string EstadoSeguimiento { get; set; } = EstadosSeguimiento.Ninguno;
        public bool MeSigue { get; set; }
    }

    // Solicitud de seguimiento que me llegó
    public class SolicitudSeguimientoViewModel
    {
        public string NombreUsuario { get; set; } = "";
        public string NombreMostrar { get; set; } = "";
        public string? AvatarUrl { get; set; }
        public DateTime FechaSolicitud { get; set; }
    }

    // Perfil de otra persona. Hereda todo lo del perfil propio (mismo diseño)
    public class PerfilPublicoViewModel : PerfilViewModel
    {
        public bool EsMio { get; set; }
        public string EstadoSeguimiento { get; set; } = EstadosSeguimiento.Ninguno;
        public bool MeSigue { get; set; }

        // false = cuenta privada que no sigo: solo se ve el encabezado
        public bool PuedeVer { get; set; }

        // El conteo de aventuras sale de la BD, porque si no puedo verlas la lista viene vacía
        public int CantidadAventurasVisibles { get; set; }

        public string TextoBotonSeguir => EstadoSeguimiento switch
        {
            EstadosSeguimiento.Aceptado => "Siguiendo",
            EstadosSeguimiento.Pendiente => "Solicitado",
            _ => MeSigue ? "Seguir también" : "Seguir"
        };
    }
}
