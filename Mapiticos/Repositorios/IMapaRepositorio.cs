using Mapiticos.Models.ViewModels;

namespace Mapiticos.Repositorios
{
    public interface IMapaRepositorio
    {
        Task<List<LugarMapaViewModel>> ListarLugaresAsync(string usuarioId);
        Task<int?> ObtenerLugarDeAventuraAsync(int aventuraId);
        Task<List<ProvinciaViewModel>> ListarProvinciasAsync();
        Task<List<EtiquetaViewModel>> ListarEtiquetasAsync();
        Task<int> CrearPuntoAsync(string usuarioId, CrearPuntoViewModel datos);   // devuelve el AventuraId nuevo
    }
}