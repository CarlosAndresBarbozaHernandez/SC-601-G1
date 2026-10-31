using System.Security.Claims;
using Mapiticos.Filtros;
using Mapiticos.Models.ViewModels;
using Mapiticos.Repositorios;
using Microsoft.AspNetCore.Mvc;

namespace Mapiticos.Controllers
{
    public class PerfilController : Controller
    {
        private readonly IPerfilRepositorio _perfilRepositorio;

        public PerfilController(IPerfilRepositorio perfilRepositorio)
        {
            _perfilRepositorio = perfilRepositorio;
        }

        private string? UsuarioActualId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        // ===== Ver perfil =====
        public async Task<IActionResult> Index()
        {
            var usuarioId = UsuarioActualId();
            if (usuarioId == null) return Challenge();

            var perfil = await _perfilRepositorio.ObtenerPerfilAsync(usuarioId);
            if (perfil == null) return NotFound();

            return View(perfil);
        }

        // ===== Editar perfil: mostrar el formulario =====
        [HttpGet]
        public async Task<IActionResult> Editar()
        {
            var usuarioId = UsuarioActualId();
            if (usuarioId == null) return Challenge();

            var modelo = await _perfilRepositorio.ObtenerParaEditarAsync(usuarioId);
            if (modelo == null) return NotFound();

            return View(modelo);
        }

        // ===== Editar perfil: guardar =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(EditarPerfilViewModel modelo)
        {
            var usuarioId = UsuarioActualId();
            if (usuarioId == null) return Challenge();

            if (!AvatarValidador.EsValido(modelo.AvatarUrl))
            {
                ModelState.AddModelError(nameof(modelo.AvatarUrl), "Ese avatar no es válido. Probá creándolo de nuevo.");
            }

            if (!ModelState.IsValid)
            {
                var correo = User.Identity?.Name ?? "";
                modelo.NombreUsuario = correo.Contains('@') ? correo.Split('@')[0] : correo;
                return View(modelo);
            }

            await _perfilRepositorio.GuardarAsync(usuarioId, modelo);

            TempData["Mensaje"] = "¡Listo! Tu perfil se actualizó.";
            return RedirectToAction(nameof(Index));
        }

        // ===== Elegir @usuario (cuentas nuevas que todavía no tienen uno) =====
        // El filtro RequiereNombreUsuarioFiltro manda aquí a quien no tiene @usuario.
        // [SinNombreUsuarioRequerido] evita que el filtro redirija en un ciclo infinito.

        [HttpGet]
        [SinNombreUsuarioRequerido]
        public async Task<IActionResult> ElegirUsuario()
        {
            var usuarioId = UsuarioActualId();
            if (usuarioId == null) return Challenge();

            // Si ya tiene @usuario, no tiene nada que hacer aquí
            if (await _perfilRepositorio.ObtenerNombreUsuarioAsync(usuarioId) != null)
                return RedirectToAction("Index", "Feed");

            return View(new ElegirUsuarioViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SinNombreUsuarioRequerido]
        public async Task<IActionResult> ElegirUsuario(ElegirUsuarioViewModel modelo)
        {
            var usuarioId = UsuarioActualId();
            if (usuarioId == null) return Challenge();

            modelo.NombreUsuario = (modelo.NombreUsuario ?? "").Trim().TrimStart('@').ToLowerInvariant();
            ModelState.Clear();
            TryValidateModel(modelo);

            if (ModelState.IsValid && ReglasNombreUsuario.EsReservado(modelo.NombreUsuario))
            {
                ModelState.AddModelError(nameof(modelo.NombreUsuario), "Ese nombre está reservado. Probá con otro.");
            }

            if (ModelState.IsValid &&
                !await _perfilRepositorio.NombreUsuarioDisponibleAsync(modelo.NombreUsuario, usuarioId))
            {
                ModelState.AddModelError(nameof(modelo.NombreUsuario), "Ese @usuario ya lo tiene otra persona.");
            }

            if (!ModelState.IsValid) return View(modelo);

            // Por si dos personas eligen el mismo nombre al mismo tiempo
            if (!await _perfilRepositorio.AsignarNombreUsuarioAsync(usuarioId, modelo.NombreUsuario))
            {
                ModelState.AddModelError(nameof(modelo.NombreUsuario), "Ese @usuario ya lo tiene otra persona.");
                return View(modelo);
            }

            return RedirectToAction("Index", "Feed");
        }
    }
}