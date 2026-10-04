using Mapiticos.Models.ViewModels;

namespace Mapiticos.Repositorios
{
    public interface ILugarRepositorio
    {
        // null = el lugar no existe o el usuario no puede ver nada ahí
        Task<LugarDetalleViewModel?> ObtenerAsync(int lugarId, string usuarioId, string filtro);

        // null = la aventura no existe o el usuario no la puede ver
        Task<LikeResultadoViewModel?> AlternarLikeAsync(string usuarioId, int aventuraId);

        // null = el lugar no existe o el usuario no lo puede ver
        Task<FavoritoResultadoViewModel?> AlternarFavoritoAsync(string usuarioId, int lugarId);

        // Agrega mi aventura a un lugar que ya existe; devuelve el AventuraId nuevo
        Task<int> AgregarAventuraAsync(string usuarioId, AgregarAventuraViewModel datos);

        // Guarda la URL de una foto de mi aventura
        Task AgregarFotoAsync(int aventuraId, string usuarioId, string url, int orden);

        // Borra una foto de mi aventura; devuelve su Url (null = no existe o no es mía)
        Task<string?> EliminarFotoAsync(int fotoId, string usuarioId);
    }
}