using Mapiticos.Models.ViewModels;

namespace Mapiticos.Repositorios
{
    public interface IPerfilRepositorio
    {
        Task<PerfilViewModel?> ObtenerPerfilAsync(string usuarioId);
        Task<EditarPerfilViewModel?> ObtenerParaEditarAsync(string usuarioId);

        // Devuelve false si el @usuario ya lo tiene otra persona
        Task<bool> GuardarAsync(string usuarioId, EditarPerfilViewModel datos);

        Task<string?> ObtenerNombreUsuarioAsync(string usuarioId);
        Task<bool> NombreUsuarioDisponibleAsync(string nombreUsuario, string usuarioId);
        Task<bool> AsignarNombreUsuarioAsync(string usuarioId, string nombreUsuario);

        // Modo (tema) de la app
        Task<string?> ObtenerTemaAsync(string usuarioId);
        Task GuardarTemaAsync(string usuarioId, string tema);
    }
}