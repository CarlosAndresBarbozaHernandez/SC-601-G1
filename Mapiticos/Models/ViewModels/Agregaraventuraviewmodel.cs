using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Mapiticos.Models.ViewModels
{
    // Formulario "Agregar mi aventura" a un lugar que ya existe
    public class AgregarAventuraViewModel
    {
        public int LugarId { get; set; }

        [StringLength(1000, ErrorMessage = "Máximo 1000 caracteres.")]
        [Display(Name = "¿Cómo te fue?")]
        public string? Descripcion { get; set; }

        public bool EsPublica { get; set; } = true;

        public List<int> EtiquetaIds { get; set; } = new();   // etiquetas personales

        public List<IFormFile> Fotos { get; set; } = new();

        // ----- Solo para mostrar en la pantalla (no vienen del formulario) -----
        [ValidateNever] public string NombreLugar { get; set; } = "";
        [ValidateNever] public string Provincia { get; set; } = "";
        [ValidateNever] public List<EtiquetaViewModel> EtiquetasLugar { get; set; } = new();
        [ValidateNever] public List<EtiquetaViewModel> Catalogo { get; set; } = new();
    }
}