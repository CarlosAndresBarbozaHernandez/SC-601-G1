using System.Security.Claims;
using Mapiticos.Models.ViewModels;
using Mapiticos.Repositorios;
using Mapiticos.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Mapiticos.Controllers
{
    public class LugaresController : Controller
    {
        private const int MaximoFotos = 5;

        private readonly ILugarRepositorio _lugarRepositorio;
        private readonly IMapaRepositorio _mapaRepositorio;   // para el catálogo de etiquetas
        private readonly IAlmacenFotos _almacenFotos;

        public LugaresController(
            ILugarRepositorio lugarRepositorio,
            IMapaRepositorio mapaRepositorio,
            IAlmacenFotos almacenFotos)
        {
            _lugarRepositorio = lugarRepositorio;
            _mapaRepositorio = mapaRepositorio;
            _almacenFotos = almacenFotos;
        }

        private string? UsuarioActualId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        // GET /Lugares/Ver/5               -> pestaña "Todas"
        // GET /Lugares/Ver/5?filtro=amigos -> pestaña "Amigos"
        public async Task<IActionResult> Ver(int id, string? filtro)
        {
            var usuarioId = UsuarioActualId();
            if (usuarioId == null) return Challenge();

            // Solo se aceptan esos dos valores; cualquier otra cosa cuenta como "todas"
            filtro = filtro == "amigos" ? "amigos" : "todas";

            var lugar = await _lugarRepositorio.ObtenerAsync(id, usuarioId, filtro);

            // No existe, o es de alguien privado que no sigo: para el usuario "no existe"
            if (lugar == null) return NotFound();

            return View(lugar);
        }

        // ===== Agregar mi aventura a este lugar =====

        // GET /Lugares/Agregar/5  -> muestra el formulario
        [HttpGet]
        public async Task<IActionResult> Agregar(int id)
        {
            var usuarioId = UsuarioActualId();
            if (usuarioId == null) return Challenge();

            var modelo = new AgregarAventuraViewModel { LugarId = id };
            if (!await CargarDatosLugarAsync(modelo, usuarioId)) return NotFound();

            return View(modelo);
        }

        // POST /Lugares/Agregar  -> guarda la aventura y sus fotos
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(30 * 1024 * 1024)]   // 5 fotos de 5 MB + el resto del formulario
        public async Task<IActionResult> Agregar(AgregarAventuraViewModel modelo)
        {
            var usuarioId = UsuarioActualId();
            if (usuarioId == null) return Challenge();

            // Reglas del formulario
            if (string.IsNullOrWhiteSpace(modelo.Descripcion) && modelo.Fotos.Count == 0)
                ModelState.AddModelError(nameof(modelo.Descripcion), "Escribí algo o subí al menos una foto.");

            if (modelo.Fotos.Count > MaximoFotos)
                ModelState.AddModelError(nameof(modelo.Fotos), $"Podés subir máximo {MaximoFotos} fotos.");

            foreach (var foto in modelo.Fotos)
            {
                var error = _almacenFotos.Validar(foto);
                if (error != null) ModelState.AddModelError(nameof(modelo.Fotos), error);
            }

            // Carga nombre del lugar y etiquetas; también confirma que el usuario puede verlo
            if (!await CargarDatosLugarAsync(modelo, usuarioId)) return NotFound();

            if (!ModelState.IsValid) return View(modelo);

            var aventuraId = await _lugarRepositorio.AgregarAventuraAsync(usuarioId, modelo);

            var orden = 1;
            foreach (var foto in modelo.Fotos)
            {
                var url = await _almacenFotos.GuardarAsync(foto);
                await _lugarRepositorio.AgregarFotoAsync(aventuraId, usuarioId, url, orden++);
            }

            TempData["Mensaje"] = "¡Tu aventura quedó publicada!";
            return RedirectToAction(nameof(Ver), new { id = modelo.LugarId });
        }

        // Llena los datos de solo lectura del formulario. false = el lugar no se puede ver
        private async Task<bool> CargarDatosLugarAsync(AgregarAventuraViewModel modelo, string usuarioId)
        {
            var lugar = await _lugarRepositorio.ObtenerAsync(modelo.LugarId, usuarioId, "todas");
            if (lugar == null) return false;

            modelo.NombreLugar = lugar.Nombre;
            modelo.Provincia = lugar.Provincia;
            modelo.EtiquetasLugar = lugar.Etiquetas;
            modelo.Catalogo = await _mapaRepositorio.ListarEtiquetasAsync();
            return true;
        }

        // ===== Subir fotos a una aventura mía =====
        // La usan: el formulario "+" del mapa (después de crear el lugar)
        // y el botón "Agregar fotos" de mis aventuras en la pantalla del lugar
        // POST /Lugares/Fotos
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(30 * 1024 * 1024)]
        public async Task<IActionResult> Fotos(int aventuraId, List<IFormFile> fotos)
        {
            var usuarioId = UsuarioActualId();
            if (usuarioId == null) return Challenge();

            if (fotos.Count == 0) return BadRequest("No llegó ninguna foto.");
            if (fotos.Count > MaximoFotos) return BadRequest($"Podés subir máximo {MaximoFotos} fotos.");

            foreach (var foto in fotos)
            {
                var error = _almacenFotos.Validar(foto);
                if (error != null) return BadRequest(error);
            }

            try
            {
                var orden = 1;
                foreach (var foto in fotos)
                {
                    var url = await _almacenFotos.GuardarAsync(foto);
                    await _lugarRepositorio.AgregarFotoAsync(aventuraId, usuarioId, url, orden++);
                }
            }
            catch (SqlException ex) when (ex.Number == 50002)
            {
                return NotFound();   // la aventura no existe o no es de este usuario
            }
            catch (SqlException ex) when (ex.Number == 50003)
            {
                return BadRequest("Cada aventura puede tener máximo 5 fotos.");
            }

            return Ok();
        }

        // ===== Borrar una foto de mi aventura =====
        // POST /Lugares/EliminarFoto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarFoto(int fotoId)
        {
            var usuarioId = UsuarioActualId();
            if (usuarioId == null) return Challenge();

            // Primero se borra de la base; si no es mía, no devuelve nada
            var url = await _lugarRepositorio.EliminarFotoAsync(fotoId, usuarioId);
            if (url == null) return NotFound();

            await _almacenFotos.EliminarAsync(url);
            return Ok();
        }

        // ===== Botones (los llaman lugar.js y mapa.js con fetch) =====

        // POST /Lugares/Like
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Like(int aventuraId)
        {
            var usuarioId = UsuarioActualId();
            if (usuarioId == null) return Challenge();

            var resultado = await _lugarRepositorio.AlternarLikeAsync(usuarioId, aventuraId);
            if (resultado == null) return NotFound();

            return Json(resultado);
        }

        // POST /Lugares/Favorito
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Favorito(int lugarId)
        {
            var usuarioId = UsuarioActualId();
            if (usuarioId == null) return Challenge();

            var resultado = await _lugarRepositorio.AlternarFavoritoAsync(usuarioId, lugarId);
            if (resultado == null) return NotFound();

            return Json(resultado);
        }
    }
}