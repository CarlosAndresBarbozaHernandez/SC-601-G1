using System.Security.Claims;
using Mapiticos.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Mapiticos.ViewComponents
{
    // Punto morado del menú: solo se ve si hay avisos sin leer.
    public class PuntoAvisosViewComponent : ViewComponent
    {
        private readonly INotificacionRepositorio _notificacionRepositorio;

        public PuntoAvisosViewComponent(INotificacionRepositorio notificacionRepositorio)
        {
            _notificacionRepositorio = notificacionRepositorio;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var usuarioId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (usuarioId == null) return Content("");

            try
            {
                var sinLeer = await _notificacionRepositorio.ContarSinLeerAsync(usuarioId);
                return sinLeer > 0 ? View() : Content("");
            }
            catch (SqlException)
            {
                // El menú sale en todas las páginas: si falla el punto
                // (por ejemplo, falta correr 14.Avisos.sql), la app sigue funcionando
                return Content("");
            }
        }
    }
}
