using Mapiticos.Servicios;

namespace Mapiticos.Models.ViewModels
{
    // Datos de la pantalla "Modo de la app"
    public class ModoViewModel
    {
        public string TemaSeleccionado { get; set; } = CatalogoTemas.Predeterminado;
        public IReadOnlyList<TemaApp> Temas { get; set; } = CatalogoTemas.Todos;
    }
}