using System.Security.Claims;
using Mapiticos.Models.ViewModels;
using Mapiticos.Repositorios;
using Microsoft.AspNetCore.Mvc;

namespace Mapiticos.Controllers
{
    // Salidas planeadas a un lugar favorito, con amigos invitados y chat.
    public class EventosController : Controller
    {
        private const int MaximoMensaje = 500;

        private readonly IEventoRepositorio _eventoRepositorio;
        private readonly ILugarRepositorio _lugarRepositorio;   // para la lista de favoritos del popup

        public EventosController(IEventoRepositorio eventoRepositorio, ILugarRepositorio lugarRepositorio)
        {
            _eventoRepositorio = eventoRepositorio;
            _lugarRepositorio = lugarRepositorio;
        }

        private string? UsuarioActualId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        // GET /Eventos?filtro=pendientes|programados|hechos
        public async Task<IActionResult> Index(string? filtro)
        {
            var usuarioId = UsuarioActualId();
            if (usuarioId == null) return Challenge();

            var modelo = new EventosIndexViewModel
            {
                Eventos = await _eventoRepositorio.ListarAsync(usuarioId),
                Filtro = filtro is "pendientes" or "programados" or "hechos" ? filtro : "todos"
            };

            return View(modelo);
        }

        // GET /Eventos/Ver/5
        public async Task<IActionResult> Ver(int id)
        {
            var usuarioId = UsuarioActualId();
            if (usuarioId == null) return Challenge();

            var evento = await _eventoRepositorio.ObtenerAsync(id, usuarioId);
            if (evento == null) return NotFound();

            return View(evento);
        }

        // GET /Eventos/Amigos?eventoId=5  (JSON para el modal de invitar)
        [HttpGet]
        public async Task<IActionResult> Amigos(int? eventoId)
        {
            var usuarioId = UsuarioActualId();
            if (usuarioId == null) return Challenge();

            return Json(await _eventoRepositorio.ListarAmigosAsync(usuarioId, eventoId));
        }

        // GET /Eventos/LugaresFavoritos  (JSON para el selector "¿A dónde van?")
        [HttpGet]
        public async Task<IActionResult> LugaresFavoritos()
        {
            var usuarioId = UsuarioActualId();
            if (usuarioId == null) return Challenge();

            var favoritos = await _lugarRepositorio.ListarFavoritosAsync(usuarioId);
            return Json(favoritos
                .OrderBy(f => f.Nombre)
                .Select(f => new { f.LugarId, f.Nombre, f.Provincia }));
        }

        // POST /Eventos/Crear  (AJAX desde Favoritos)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CrearEventoViewModel datos)
        {
            var usuarioId = UsuarioActualId();
            if (usuarioId == null) return Challenge();

            // Se deja un margen de 1 hora por si alguien planea "ya mismo"
            if (datos.FechaEvento.HasValue && datos.FechaEvento.Value < DateTime.Now.AddHours(-1))
                ModelState.AddModelError(nameof(datos.FechaEvento), "La fecha tiene que ser en el futuro.");

            if (!ModelState.IsValid)
            {
                var primerError = ModelState.Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage).FirstOrDefault(m => !string.IsNullOrEmpty(m));
                return BadRequest(new { mensaje = primerError ?? "Revisá los datos del evento." });
            }

            try
            {
                var eventoId = await _eventoRepositorio.CrearAsync(usuarioId, datos);
                return Json(new { exito = true, url = Url.Action(nameof(Ver), new { id = eventoId }) });
            }
            catch (EventoException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // POST /Eventos/Invitar  (AJAX desde Ver)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Invitar(int eventoId, List<string> invitados)
        {
            var usuarioId = UsuarioActualId();
            if (usuarioId == null) return Challenge();

            if (invitados == null || invitados.Count == 0)
                return BadRequest(new { mensaje = "Elegí al menos un amigo." });

            try
            {
                await _eventoRepositorio.InvitarAsync(eventoId, usuarioId, invitados);
                return Json(new { exito = true });
            }
            catch (EventoException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // ----- Acciones con formulario normal (redirigen a Ver o a la lista) -----

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> Responder(int id, bool acepta) =>
            EjecutarYVolverAsync(id,
                u => _eventoRepositorio.ResponderAsync(id, u, acepta),
                acepta ? "¡Listo! Ya estás en el evento." : "Rechazaste la invitación.",
                volverALista: !acepta);   // si rechaza, ya no puede ver el evento

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> Salir(int id) =>
            EjecutarYVolverAsync(id,
                u => _eventoRepositorio.SalirAsync(id, u),
                "Saliste del evento.",
                volverALista: true);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> CambiarEstado(int id, bool hecho) =>
            EjecutarYVolverAsync(id,
                u => _eventoRepositorio.CambiarEstadoAsync(id, u, hecho),
                hecho ? "¡Qué buena salida! El evento quedó como hecho." : "El evento volvió a programado.");

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> Eliminar(int id) =>
            EjecutarYVolverAsync(id,
                u => _eventoRepositorio.EliminarAsync(id, u),
                "El evento se canceló.",
                volverALista: true);

        // Patrón común: ejecutar la acción, guardar el mensaje y redirigir
        private async Task<IActionResult> EjecutarYVolverAsync(
            int id, Func<string, Task> accion, string mensajeExito, bool volverALista = false)
        {
            var usuarioId = UsuarioActualId();
            if (usuarioId == null) return Challenge();

            try
            {
                await accion(usuarioId);
                TempData["Mensaje"] = mensajeExito;
            }
            catch (EventoException ex)
            {
                TempData["Error"] = ex.Message;
                volverALista = false;
            }

            return volverALista
                ? RedirectToAction(nameof(Index))
                : RedirectToAction(nameof(Ver), new { id });
        }

        // ----- Chat -----

        // GET /Eventos/Mensajes?eventoId=5&desdeId=12  (solo los nuevos)
        [HttpGet]
        public async Task<IActionResult> Mensajes(int eventoId, int desdeId = 0)
        {
            var usuarioId = UsuarioActualId();
            if (usuarioId == null) return Challenge();

            return Json(await _eventoRepositorio.ListarMensajesAsync(eventoId, usuarioId, desdeId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarMensaje(int eventoId, string texto)
        {
            var usuarioId = UsuarioActualId();
            if (usuarioId == null) return Challenge();

            texto = texto?.Trim() ?? "";
            if (texto.Length == 0 || texto.Length > MaximoMensaje)
                return BadRequest(new { mensaje = $"El mensaje debe tener entre 1 y {MaximoMensaje} caracteres." });

            try
            {
                await _eventoRepositorio.AgregarMensajeAsync(eventoId, usuarioId, texto);
                return Json(new { exito = true });
            }
            catch (EventoException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}