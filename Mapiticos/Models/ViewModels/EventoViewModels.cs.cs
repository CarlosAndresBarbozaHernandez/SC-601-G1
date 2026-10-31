using System.ComponentModel.DataAnnotations;

namespace Mapiticos.Models.ViewModels
{
    // Valores que se guardan en la BD. Un solo lugar para no escribir strings sueltos.
    public static class EstadosEvento
    {
        // Como ve el usuario cada evento (filtros de la pantalla)
        public const string Pendiente = "Pendiente";     // me invitaron y no he respondido
        public const string Programado = "Programado";   // voy y todavía no se hizo
        public const string Hecho = "Hecho";             // ya se hizo

        // Estado de cada participante
        public const string Aceptado = "Aceptado";
        public const string Rechazado = "Rechazado";
    }

    public class ParticipanteEventoViewModel
    {
        public string NombreUsuario { get; set; } = "";
        public string NombreMostrar { get; set; } = "";
        public string? AvatarUrl { get; set; }
        public string Estado { get; set; } = "";
        public bool EsCreador { get; set; }

        public string Inicial => string.IsNullOrEmpty(NombreMostrar) ? "?" : NombreMostrar[..1].ToUpper();
    }

    // Tarjeta de la lista de eventos
    public class EventoTarjetaViewModel
    {
        public int EventoId { get; set; }
        public string Titulo { get; set; } = "";
        public string? Descripcion { get; set; }
        public DateTime FechaEvento { get; set; }
        public string EstadoEvento { get; set; } = "";
        public string MiEstado { get; set; } = "";
        public string Categoria { get; set; } = "";       // Pendiente | Programado | Hecho
        public bool SoyCreador { get; set; }
        public int LugarId { get; set; }
        public string Lugar { get; set; } = "";
        public string Provincia { get; set; } = "";
        public string CreadorNombreUsuario { get; set; } = "";
        public List<ParticipanteEventoViewModel> Participantes { get; set; } = new();

        public int CantidadAceptados => Participantes.Count(p => p.Estado == EstadosEvento.Aceptado);
        public int CantidadPendientes => Participantes.Count(p => p.Estado == EstadosEvento.Pendiente);

        // Programado pero la fecha ya pasó: se le sugiere al creador marcarlo como hecho
        public bool FechaPasada => Categoria == EstadosEvento.Programado && FechaEvento < DateTime.Now;
    }

    // Pantalla Eventos/Index
    public class EventosIndexViewModel
    {
        public List<EventoTarjetaViewModel> Eventos { get; set; } = new();
        public string Filtro { get; set; } = "todos";

        public int Contar(string categoria) => Eventos.Count(e => e.Categoria == categoria);
    }

    // Pantalla Eventos/Ver
    public class EventoDetalleViewModel : EventoTarjetaViewModel
    {
        public DateTime? FechaHecho { get; set; }

        public bool PuedeChatear => MiEstado == EstadosEvento.Aceptado;
    }

    public class MensajeEventoViewModel
    {
        public int MensajeId { get; set; }
        public string NombreUsuario { get; set; } = "";
        public string NombreMostrar { get; set; } = "";
        public string? AvatarUrl { get; set; }
        public string Texto { get; set; } = "";
        public DateTime Fecha { get; set; }
        public bool EsMio { get; set; }
    }

    public class AmigoViewModel
    {
        public string NombreUsuario { get; set; } = "";
        public string NombreMostrar { get; set; } = "";
        public string? AvatarUrl { get; set; }
        public bool YaInvitado { get; set; }
    }

    // Lo que manda el formulario "Planear salida"
    public class CrearEventoViewModel
    {
        [Range(1, int.MaxValue)]
        public int LugarId { get; set; }

        [Required(ErrorMessage = "Ponele un nombre a la salida.")]
        [StringLength(100, ErrorMessage = "El nombre puede tener máximo 100 caracteres.")]
        public string Titulo { get; set; } = "";

        [StringLength(500, ErrorMessage = "La descripción puede tener máximo 500 caracteres.")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "Elegí la fecha y hora.")]
        public DateTime? FechaEvento { get; set; }

        // @usuarios de los amigos invitados
        public List<string> Invitados { get; set; } = new();
    }
}