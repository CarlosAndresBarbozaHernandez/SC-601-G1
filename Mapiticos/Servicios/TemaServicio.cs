using System.Security.Claims;
using Mapiticos.Repositorios;

namespace Mapiticos.Servicios
{
    // Busca el modo del usuario en la base de datos y lo guarda en una cookie,
    // así no hay que ir a la base en cada página.
    public class TemaServicio : ITemaServicio
    {
        private const string NombreCookie = "Mapiticos.Tema";
        private const string ClaveItems = "Mapiticos.TemaActual";

        private readonly IHttpContextAccessor _http;
        private readonly IPerfilRepositorio _perfilRepositorio;

        public TemaServicio(IHttpContextAccessor http, IPerfilRepositorio perfilRepositorio)
        {
            _http = http;
            _perfilRepositorio = perfilRepositorio;
        }

        public async Task<string> ObtenerTemaActualAsync()
        {
            var contexto = _http.HttpContext;
            if (contexto == null) return CatalogoTemas.Predeterminado;

            // Si ya se buscó en esta misma petición, no se busca otra vez
            if (contexto.Items[ClaveItems] is string yaBuscado) return yaBuscado;

            var usuarioId = contexto.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var tema = CatalogoTemas.Predeterminado;

            if (usuarioId != null)
            {
                // La cookie guarda "usuarioId|tema", para que no se mezcle si otra persona usa la misma compu
                var cookie = contexto.Request.Cookies[NombreCookie];
                var partes = cookie?.Split('|');

                if (partes is { Length: 2 } && partes[0] == usuarioId && CatalogoTemas.Existe(partes[1]))
                {
                    tema = partes[1];
                }
                else
                {
                    var deLaBase = await _perfilRepositorio.ObtenerTemaAsync(usuarioId);
                    tema = CatalogoTemas.Existe(deLaBase) ? deLaBase! : CatalogoTemas.Predeterminado;
                    GuardarCookie(contexto, usuarioId, tema);
                }
            }

            contexto.Items[ClaveItems] = tema;
            return tema;
        }

        public async Task GuardarTemaAsync(string tema)
        {
            var contexto = _http.HttpContext ?? throw new InvalidOperationException("No hay una petición activa.");
            var usuarioId = contexto.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException("Hay que iniciar sesión para cambiar el modo.");

            if (!CatalogoTemas.Existe(tema)) tema = CatalogoTemas.Predeterminado;

            await _perfilRepositorio.GuardarTemaAsync(usuarioId, tema);
            GuardarCookie(contexto, usuarioId, tema);
            contexto.Items[ClaveItems] = tema;
        }

        private static void GuardarCookie(HttpContext contexto, string usuarioId, string tema)
        {
            if (contexto.Response.HasStarted) return;   // ya no se pueden agregar cookies

            contexto.Response.Cookies.Append(NombreCookie, $"{usuarioId}|{tema}", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                IsEssential = true,
                Expires = DateTimeOffset.UtcNow.AddDays(180)
            });
        }
    }
}