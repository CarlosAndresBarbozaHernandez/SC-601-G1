
using System.Security.Claims;
using Mapiticos.Repositorios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mapiticos.Controllers
{
    [Authorize]
    public class FeedController : Controller
    {
        private readonly ILugarRepositorio _lugarRepositorio;

        public FeedController(ILugarRepositorio lugarRepositorio)
        {
            _lugarRepositorio = lugarRepositorio;
        }

        //  Mostrar publicaciones del Feed 
        public async Task<IActionResult> Index()
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (usuarioId == null)
                return Challenge();

            // Obtiene las publicaciones reales desde SQL Server
            var publicaciones = await _lugarRepositorio.ListarFeedAsync(usuarioId);

            return View(publicaciones);
        }

        //  Consultar comentarios de una aventura 
        [HttpGet]
        public async Task<IActionResult> Comentarios(int aventuraId)
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (usuarioId == null)
                return Challenge();

            if (aventuraId <= 0)
                return BadRequest();

            var comentarios = await _lugarRepositorio
                .ListarComentariosAsync(usuarioId, aventuraId);

            return Json(comentarios);
        }

        // Publicar un comentario 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarComentario(
            int aventuraId, string texto)
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (usuarioId == null)
                return Challenge();

            if (aventuraId <= 0 ||
                string.IsNullOrWhiteSpace(texto) ||
                texto.Trim().Length > 500)
            {
                return BadRequest(new
                {
                    mensaje = "El comentario debe contener entre 1 y 500 caracteres."
                });
            }

            await _lugarRepositorio.AgregarComentarioAsync(
                usuarioId, aventuraId, texto.Trim());

            return Json(new
            {
                exito = true,
                mensaje = "Comentario publicado correctamente."
            });
        }
    }
}
