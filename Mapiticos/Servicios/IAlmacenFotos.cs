namespace Mapiticos.Servicios
{
    // Dónde y cómo se guardan las fotos.
    // Hoy se guardan en la carpeta wwwroot/uploads; si un día se pasan a la nube,
    // se hace otra clase que implemente esta interfaz y el controller no cambia.
    public interface IAlmacenFotos
    {
        // null = la foto está bien; si no, el mensaje de error para mostrar
        string? Validar(IFormFile archivo);

        // Guarda la foto y devuelve la URL para el <img>, ej: /uploads/aventuras/abc123.jpg
        Task<string> GuardarAsync(IFormFile archivo);

        // Borra el archivo de una foto a partir de su URL
        Task EliminarAsync(string url);
    }
}