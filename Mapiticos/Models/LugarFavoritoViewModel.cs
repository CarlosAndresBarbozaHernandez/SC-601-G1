namespace Mapiticos.Models.ViewModels
{
    public class LugarFavoritoViewModel
    {
        public int LugarId { get; set; }
        public string Nombre { get; set; } = "";
        public string Provincia { get; set; } = "";
        public string Estado { get; set; } = "";
        public DateTime FechaAgregado { get; set; }
    }
}