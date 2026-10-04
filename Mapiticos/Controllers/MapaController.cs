using System.Security.Claims;
using Mapiticos.Models.ViewModels;
using Mapiticos.Repositorios;
using Microsoft.AspNetCore.Mvc;

namespace Mapiticos.Controllers
{
    public class MapaController : Controller
    {
        private readonly IMapaRepositorio _mapaRepositorio;

        public MapaController(IMapaRepositorio mapaRepositorio)
        {
            _mapaRepositorio = mapaRepositorio;
        }

        private string? UsuarioActualId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        // La página del mapa.
        // aventuraId viene de "Ver en el mapa" (Feed / Perfil); se convierte al lugar donde está
        public async Task<IActionResult> Index(int? aventuraId, int? lugarId)
        {
            if (lugarId == null && aventuraId != null)
            {
                lugarId = await _mapaRepositorio.ObtenerLugarDeAventuraAsync(aventuraId.Value);
            }

            var modelo = new MapaViewModel
            {
                LugarId = lugarId,
                Provincias = await _mapaRepositorio.ListarProvinciasAsync(),
                Etiquetas = await _mapaRepositorio.ListarEtiquetasAsync()
            };
            return View(modelo);
        }

        // Los pines (lugares) en JSON; los pide mapa.js
        [HttpGet]
        public async Task<IActionResult> Puntos()
        {
            var usuarioId = UsuarioActualId();
            if (usuarioId == null) return Challenge();

            return Json(await _mapaRepositorio.ListarLugaresAsync(usuarioId));
        }

        // Guarda un lugar nuevo con la primera aventura.
        // Recibe JSON; el token anti-falsificación viaja en el header RequestVerificationToken
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear([FromBody] CrearPuntoViewModel modelo)
        {
            var usuarioId = UsuarioActualId();
            if (usuarioId == null) return Challenge();

            if (!ModelState.IsValid) return BadRequest();

            var aventuraId = await _mapaRepositorio.CrearPuntoAsync(usuarioId, modelo);
            var lugarId = await _mapaRepositorio.ObtenerLugarDeAventuraAsync(aventuraId);
            return Ok(new { aventuraId, lugarId });
        }
    }
}