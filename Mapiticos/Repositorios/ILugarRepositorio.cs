
using Mapiticos.Models.ViewModels;

namespace Mapiticos.Repositorios
{
    public interface ILugarRepositorio
    {
        Task<LugarDetalleViewModel?> ObtenerAsync(int lugarId, string usuarioId, string filtro);

        Task<LikeResultadoViewModel?> AlternarLikeAsync(string usuarioId, int aventuraId);

        Task<FavoritoResultadoViewModel?> AlternarFavoritoAsync(string usuarioId, int lugarId);

        Task<int> AgregarAventuraAsync(string usuarioId, AgregarAventuraViewModel datos);

        Task AgregarFotoAsync(int aventuraId, string usuarioId, string url, int orden);

        Task<string?> EliminarFotoAsync(int fotoId, string usuarioId);

        Task<List<LugarFavoritoViewModel>> ListarFavoritosAsync(string usuarioId);

        // Obtiene las publicaciones reales del Feed
        Task<List<PublicacionFeedViewModel>> ListarFeedAsync(string usuarioId);

        Task<List<ComentarioFeedViewModel>> ListarComentariosAsync(
            string usuarioId, int aventuraId);

        Task AgregarComentarioAsync(
            string usuarioId, int aventuraId, string texto);

    }
}
