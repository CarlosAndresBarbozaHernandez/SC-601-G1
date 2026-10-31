using Mapiticos.Models.ViewModels;

namespace Mapiticos.Repositorios
{
    // Acceso a datos de Eventos (salidas planeadas a un lugar con amigos).
    // Los errores de regla de negocio llegan como EventoException con un mensaje para el usuario.
    public interface IEventoRepositorio
    {
        Task<List<AmigoViewModel>> ListarAmigosAsync(string usuarioId, int? eventoId = null);

        Task<int> CrearAsync(string creadorId, CrearEventoViewModel datos);

        Task InvitarAsync(int eventoId, string usuarioId, IEnumerable<string> invitados);

        Task<List<EventoTarjetaViewModel>> ListarAsync(string usuarioId);

        Task<EventoDetalleViewModel?> ObtenerAsync(int eventoId, string usuarioId);

        Task ResponderAsync(int eventoId, string usuarioId, bool acepta);

        Task SalirAsync(int eventoId, string usuarioId);

        Task CambiarEstadoAsync(int eventoId, string usuarioId, bool hecho);

        Task EliminarAsync(int eventoId, string usuarioId);

        Task<List<MensajeEventoViewModel>> ListarMensajesAsync(int eventoId, string usuarioId, int desdeId);

        Task AgregarMensajeAsync(int eventoId, string usuarioId, string texto);

        Task<int> ContarPendientesAsync(string usuarioId);
    }

 
    public class EventoException : Exception
    {
        public EventoException(string mensaje, Exception? interna = null) : base(mensaje, interna) { }
    }
}