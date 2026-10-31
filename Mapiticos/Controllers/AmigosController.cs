using System.Security.Claims;
using Mapiticos.Repositorios;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;

namespace Mapiticos.Controllers
{
    // Perfil de otras personas + endpoints JSON del buscador del Feed.
    public class AmigosController : Controller
    {
        private readonly IAmigosRepositorio _amigosRepositorio;

        public AmigosController(IAmigosRepositorio amigosRepositorio)
        {
            _amigosRepositorio = amigosRepositorio;
        }

        private string? UsuarioActualId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Error de SQL con un mensaje que dice qué falta (se ve en el popup)
        private IActionResult ErrorBaseDatos(SqlException ex) =>
            StatusCode(500, new
            {
                mensaje = ex.Number == 2812   // 2812 = no existe el procedimiento
                    ? "Falta correr 13.Amigos.sql en SQL Server (no existe " + ex.Message.Split('\'').ElementAtOrDefault(1) + ")."
                    : "Error de base de datos: " + ex.Message
            });

        // GET /Amigos/Perfil/ana   -> perfil de otra persona (tipo Instagram)
        public async Task<IActionResult> Perfil(string id)
        {
            var usuarioId = UsuarioActualId();
            if (usuarioId == null) return Challenge();

            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var perfil = await _amigosRepositorio.ObtenerPerfilPublicoAsync(usuarioId, id.Trim().TrimStart('@'));
            if (perfil == null) return NotFound();

            // Si es mi propio perfil, voy a la pantalla de siempre (con Editar perfil)
            if (perfil.EsMio) return RedirectToAction("Index", "Perfil");

            return View(perfil);
        }

        // GET /Amigos/Buscar?texto=ana
        [HttpGet]
        public async Task<IActionResult> Buscar(string? texto)
        {
            var usuarioId = UsuarioActualId();
            if (usuarioId == null) return Challenge();

            texto = texto?.Trim() ?? "";
            if (texto.TrimStart('@').Length < 2)
                return Json(Array.Empty<object>());   // mínimo 2 letras

            if (texto.Length > 100) texto = texto[..100];

            try
            {
                return Json(await _amigosRepositorio.BuscarAsync(usuarioId, texto));
            }
            catch (SqlException ex)
            {
                return ErrorBaseDatos(ex);
            }
        }

        // POST /Amigos/Seguir   (seguir, dejar de seguir o cancelar solicitud)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Seguir(string nombreUsuario)
        {
            var usuarioId = UsuarioActualId();
            if (usuarioId == null) return Challenge();

            if (string.IsNullOrWhiteSpace(nombreUsuario))
                return BadRequest(new { mensaje = "Falta la persona." });

            try
            {
                var estado = await _amigosRepositorio.AlternarSeguimientoAsync(usuarioId, nombreUsuario.Trim());
                return Json(new { estadoSeguimiento = estado });
            }
            catch (AmigosException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // GET /Amigos/Solicitudes
        [HttpGet]
        public async Task<IActionResult> Solicitudes()
        {
            var usuarioId = UsuarioActualId();
            if (usuarioId == null) return Challenge();

            try
            {
                return Json(await _amigosRepositorio.ListarSolicitudesAsync(usuarioId));
            }
            catch (SqlException ex)
            {
                return ErrorBaseDatos(ex);
            }
        }

        // POST /Amigos/ResponderSolicitud
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResponderSolicitud(string nombreUsuario, bool acepta)
        {
            var usuarioId = UsuarioActualId();
            if (usuarioId == null) return Challenge();

            try
            {
                await _amigosRepositorio.ResponderSolicitudAsync(usuarioId, nombreUsuario, acepta);
                return Json(new { exito = true });
            }
            catch (AmigosException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}