using Mapiticos.Models.ViewModels;

namespace Mapiticos.Repositorios
{
    // Los avisos que le llegan a cada persona (tabla Notificaciones).
    public interface INotificacionRepositorio
    {
        Task<List<NotificacionViewModel>> ListarAsync(string usuarioId);

        Task MarcarLeidasAsync(string usuarioId);

        Task<int> ContarSinLeerAsync(string usuarioId);
    }
}
