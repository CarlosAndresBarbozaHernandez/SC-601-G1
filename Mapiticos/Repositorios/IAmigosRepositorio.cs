using Mapiticos.Models.ViewModels;

namespace Mapiticos.Repositorios
{
    // Buscar personas, seguir/dejar de seguir y responder solicitudes.
    // Los errores de regla de negocio (THROW 52xxx) llegan como AmigosException.
    public interface IAmigosRepositorio
    {
        Task<List<UsuarioBusquedaViewModel>> BuscarAsync(string usuarioId, string texto);

        // Devuelve el estado nuevo: Ninguno | Pendiente | Aceptado
        Task<string> AlternarSeguimientoAsync(string usuarioId, string nombreUsuario);

        Task<List<SolicitudSeguimientoViewModel>> ListarSolicitudesAsync(string usuarioId);

        Task ResponderSolicitudAsync(string usuarioId, string nombreUsuario, bool acepta);

        // null = no existe ese @usuario
        Task<PerfilPublicoViewModel?> ObtenerPerfilPublicoAsync(string visitanteId, string nombreUsuario);
    }

    public class AmigosException : Exception
    {
        public AmigosException(string mensaje, Exception? interna = null) : base(mensaje, interna) { }
    }
}
