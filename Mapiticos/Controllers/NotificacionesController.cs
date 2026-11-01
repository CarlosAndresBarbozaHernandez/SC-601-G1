using System.Security.Claims;
using Mapiticos.Repositorios;
using Microsoft.AspNetCore.Mvc;

namespace Mapiticos.Controllers
{
    // Pantalla de Avisos.
    public class NotificacionesController : Controller
    {
        private readonly INotificacionRepositorio _notificacionRepositorio;

        public NotificacionesController(INotificacionRepositorio notificacionRepositorio)
        {
            _notificacionRepositorio = notificacionRepositorio;
        }

        // GET /Notificaciones
        public async Task<IActionResult> Index()
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (usuarioId == null) return Challenge();

            // Primero se listan, para que esta vez todavía se vean como nuevos
            var avisos = await _notificacionRepositorio.ListarAsync(usuarioId);
            await _notificacionRepositorio.MarcarLeidasAsync(usuarioId);

            return View(avisos);
        }
    }
}
