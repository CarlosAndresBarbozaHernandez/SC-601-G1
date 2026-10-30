
using System.Security.Claims;
using Mapiticos.Repositorios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mapiticos.Controllers
{
    [Authorize]
    public class FavoritosController : Controller
    {
        private readonly ILugarRepositorio _lugarRepositorio;

        public FavoritosController(ILugarRepositorio lugarRepositorio)
        {
            _lugarRepositorio = lugarRepositorio;
        }

        public async Task<IActionResult> Index()
        {
            // Obtener el ID del usuario que inició sesión
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (usuarioId == null)
            {
                return Challenge();
            }

            // Consultar los lugares favoritos del usuario
            var favoritos = await _lugarRepositorio.ListarFavoritosAsync(usuarioId);

            // Enviar los favoritos a la vista
            return View(favoritos);
        }
    }
}
