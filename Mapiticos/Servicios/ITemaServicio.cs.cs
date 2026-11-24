namespace Mapiticos.Servicios
{
    // Sabe cuál modo (tema) tiene el usuario que está usando la app.
    public interface ITemaServicio
    {
        // Devuelve la clave del modo del usuario actual, por ejemplo "bosque".
        // Si no ha iniciado sesión, devuelve el predeterminado.
        Task<string> ObtenerTemaActualAsync();

        // Guarda el modo del usuario actual
        Task GuardarTemaAsync(string tema);
    }
}