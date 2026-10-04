using System.ComponentModel.DataAnnotations;

namespace Mapiticos.Models.ViewModels
{
    public class ProvinciaViewModel
    {
        public int ProvinciaId { get; set; }
        public string Nombre { get; set; } = "";
    }

    // Un pin del mapa = un LUGAR; se manda al navegador como JSON
    public class LugarMapaViewModel
    {
        public int LugarId { get; set; }
        public string Nombre { get; set; } = "";
        public string Provincia { get; set; } = "";
        public decimal Latitud { get; set; }
        public decimal Longitud { get; set; }
        public int CantidadAventuras { get; set; }   // solo las que este usuario puede ver
        public bool TengoAventura { get; set; }      // true = yo publiqué aquí (pin de otro color)
        public bool EsFavorito { get; set; }         // true = está en mis favoritos
        public List<EtiquetaViewModel> Etiquetas { get; set; } = new();   // las fijas del lugar
    }

    // Lo que llega del formulario "Agregar punto"
    public class CrearPuntoViewModel
    {
        [Required, StringLength(100)]
        public string Titulo { get; set; } = "";

        [StringLength(1000)]
        public string? Descripcion { get; set; }

        [Range(1, int.MaxValue)]
        public int ProvinciaId { get; set; }

        // Límites aproximados de Costa Rica, para rechazar puntos fuera del país
        [Range(5.0, 11.5)]
        public decimal Latitud { get; set; }

        [Range(-87.5, -82.0)]
        public decimal Longitud { get; set; }

        public bool EsPublica { get; set; } = true;
        public List<int> EtiquetaIds { get; set; } = new();
    }

    // Lo que necesita la vista del mapa
    public class MapaViewModel
    {
        public int? LugarId { get; set; }   // lugar a enfocar (viene del Feed o del Perfil)
        public List<ProvinciaViewModel> Provincias { get; set; } = new();
        public List<EtiquetaViewModel> Etiquetas { get; set; } = new();
    }
}