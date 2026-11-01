namespace Mapiticos.Models.ViewModels
{
    // Valores de Notificaciones.Tipo (los mismos del CHECK de la tabla)
    public static class TiposNotificacion
    {
        public const string Like = "Like";
        public const string Comentario = "Comentario";
        public const string SolicitudSeguimiento = "SolicitudSeguimiento";
        public const string SolicitudAceptada = "SolicitudAceptada";
        public const string NuevoSeguidor = "NuevoSeguidor";
        public const string NuevaAventura = "NuevaAventura";
    }

    // Todo lo que la vista necesita para dibujar UN aviso
    public class NotificacionViewModel
    {
        public int NotificacionId { get; set; }
        public string Tipo { get; set; } = "";
        public bool Leida { get; set; }
        public DateTime Fecha { get; set; }
        public string NombreUsuario { get; set; } = "";
        public string NombreMostrar { get; set; } = "";
        public string? AvatarUrl { get; set; }

        // null = el aviso no es de una aventura o ya no se puede ver
        public int? LugarId { get; set; }
        public string? Lugar { get; set; }

        // Primera letra del usuario, para el avatar cuando no tiene foto
        public string Inicial =>
            string.IsNullOrEmpty(NombreUsuario) ? "?" : NombreUsuario[..1].ToUpper();

        // Lo que hizo la persona, va después de su nombre
        public string Texto => Tipo switch
        {
            TiposNotificacion.Like => "le dio like a tu aventura",
            TiposNotificacion.Comentario => "comentó tu aventura",
            TiposNotificacion.SolicitudSeguimiento => "quiere seguirte",
            TiposNotificacion.SolicitudAceptada => "aceptó tu solicitud",
            TiposNotificacion.NuevoSeguidor => "empezó a seguirte",
            TiposNotificacion.NuevaAventura => "publicó una aventura",
            _ => "hizo algo nuevo"
        };

        // Clase de Tabler para cada tipo de aviso
        public string Icono => Tipo switch
        {
            TiposNotificacion.Like => "ti-heart",
            TiposNotificacion.Comentario => "ti-message-circle",
            TiposNotificacion.SolicitudSeguimiento => "ti-user-question",
            TiposNotificacion.SolicitudAceptada => "ti-user-check",
            TiposNotificacion.NuevoSeguidor => "ti-user-plus",
            TiposNotificacion.NuevaAventura => "ti-map-pin",
            _ => "ti-bell"
        };

        // Texto tipo "Hace 2 horas"
        public string TiempoTranscurrido
        {
            get
            {
                var diferencia = DateTime.Now - Fecha;
                if (diferencia.TotalMinutes < 60) return $"Hace {(int)diferencia.TotalMinutes} min";
                if (diferencia.TotalHours < 24) return $"Hace {(int)diferencia.TotalHours} h";
                if (diferencia.TotalDays < 2) return "Ayer";
                return $"Hace {(int)diferencia.TotalDays} días";
            }
        }
    }
}
