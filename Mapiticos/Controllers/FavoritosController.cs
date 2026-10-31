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
        private readonly IEventoRepositorio _eventoRepositorio;

        public FavoritosController(ILugarRepositorio lugarRepositorio, IEventoRepositorio eventoRepositorio)
        {
            _lugarRepositorio = lugarRepositorio;
            _eventoRepositorio = eventoRepositorio;
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

            // Globito de la pestaña "Eventos": invitaciones sin responder
            ViewData["EventosPendientes"] = await _eventoRepositorio.ContarPendientesAsync(usuarioId);

            // Enviar los favoritos a la vista
            return View(favoritos);
        }
    }
}