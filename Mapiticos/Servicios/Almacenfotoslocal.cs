namespace Mapiticos.Servicios
{
    // Guarda las fotos en wwwroot/uploads/aventuras
    public class AlmacenFotosLocal : IAlmacenFotos
    {
        private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long TamanoMaximo = 5 * 1024 * 1024;   // 5 MB

        private readonly IWebHostEnvironment _entorno;

        public AlmacenFotosLocal(IWebHostEnvironment entorno)
        {
            _entorno = entorno;
        }

        public string? Validar(IFormFile archivo)
        {
            if (archivo.Length == 0)
                return "Una de las fotos está vacía.";

            if (archivo.Length > TamanoMaximo)
                return $"\"{archivo.FileName}\" pesa más de 5 MB.";

            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (!ExtensionesPermitidas.Contains(extension) || !archivo.ContentType.StartsWith("image/"))
                return $"\"{archivo.FileName}\" no es una foto JPG, PNG o WEBP.";

            return null;
        }

        public async Task<string> GuardarAsync(IFormFile archivo)
        {
            var carpeta = Path.Combine(_entorno.WebRootPath, "uploads", "aventuras");
            Directory.CreateDirectory(carpeta);   // la crea si no existe

            // Nombre aleatorio: así dos fotos "IMG_001.jpg" no se pisan
            // y nadie puede meter rutas raras en el nombre del archivo
            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            var nombre = $"{Guid.NewGuid():N}{extension}";

            using var destino = File.Create(Path.Combine(carpeta, nombre));
            await archivo.CopyToAsync(destino);

            return $"/uploads/aventuras/{nombre}";
        }

        public Task EliminarAsync(string url)
        {
            // Solo borra archivos de nuestra carpeta; las fotos de ejemplo (/img/...) no se tocan.
            // Path.GetFileName se queda solo con "abc123.jpg", así nadie puede apuntar a otra carpeta.
            if (url.StartsWith("/uploads/aventuras/"))
            {
                var ruta = Path.Combine(_entorno.WebRootPath, "uploads", "aventuras", Path.GetFileName(url));
                if (File.Exists(ruta)) File.Delete(ruta);
            }
            return Task.CompletedTask;
        }
    }
}