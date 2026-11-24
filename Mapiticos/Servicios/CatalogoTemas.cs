namespace Mapiticos.Servicios
{
    // Un modo (tema) de la app: su clave, su nombre y los colores de su logo.
    public record TemaApp(
        string Clave,
        string Nombre,
        string Descripcion,
        string ColorMenu,
        string ColorMapa,
        string ColorM,
        string ColorPin,
        string ColorHueco);

    // para agregar un modo nuevo se agrega aquí, en temas.css y en _Logo.cshtml.
    public static class CatalogoTemas
    {
        public const string Predeterminado = "bosque";

        public static readonly IReadOnlyList<TemaApp> Todos = new List<TemaApp>
        {
            new("bosque", "Bosque", "Verde, con los colores del logo.",
                ColorMenu: "#22392B", ColorMapa: "#4E6B3A", ColorM: "#F3F6EC", ColorPin: "#C9A3D6", ColorHueco: "#22392B"),
            new("claro", "Claro", "Blanco y verde, el diseño original.",
                ColorMenu: "#FFFFFF", ColorMapa: "#DCE6CB", ColorM: "#22392B", ColorPin: "#7B3F8C", ColorHueco: "#FFFFFF"),
            new("oscuro", "Oscuro", "Para la noche, en tonos oscuros.",
                ColorMenu: "#0F1612", ColorMapa: "#2F4A37", ColorM: "#DCE6CB", ColorPin: "#C9A3D6", ColorHueco: "#0F1612"),
            new("guaria", "Guaria morada", "Morado, con la flor nacional.",
                ColorMenu: "#3A1D45", ColorMapa: "#6A3478", ColorM: "#FFFFFF", ColorPin: "#9B4FB0", ColorHueco: "#3A1D45"),
            new("perezoso", "Perezoso", "Café, con un perezoso colgado.",
                ColorMenu: "#3B2A20", ColorMapa: "#6E4F38", ColorM: "#F6EEDF", ColorPin: "#D9A066", ColorHueco: "#3B2A20"),
            new("tucan", "Tucán", "Negro y naranja, con un tucán.",
                ColorMenu: "#161616", ColorMapa: "#2E4A2B", ColorM: "#FFF6E0", ColorPin: "#F28C28", ColorHueco: "#161616"),
            new("carreta", "Carreta", "Rojo y mostaza, con la rueda de carreta.",
                ColorMenu: "#8E1C17", ColorMapa: "#B8822A", ColorM: "#FFF6E0", ColorPin: "#1F5FAD", ColorHueco: "#8E1C17"),
            new("morpho", "Morpho", "Azul, con la mariposa morpho.",
                ColorMenu: "#0B2A5B", ColorMapa: "#1A4F9C", ColorM: "#FFFFFF", ColorPin: "#3FA9F5", ColorHueco: "#0B2A5B"),
        };

        public static bool Existe(string? clave) =>
            clave != null && Todos.Any(t => t.Clave == clave);


        public static TemaApp Obtener(string? clave) =>
            Todos.FirstOrDefault(t => t.Clave == clave) ?? Todos.First(t => t.Clave == Predeterminado);
    }
}