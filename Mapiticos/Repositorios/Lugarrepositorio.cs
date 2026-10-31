

using System.Data;
using Mapiticos.Models.ViewModels;
using Microsoft.Data.SqlClient;

namespace Mapiticos.Repositorios
{
    public class LugarRepositorio : ILugarRepositorio
    {
        private readonly string _cadenaConexion;

        public LugarRepositorio(IConfiguration configuracion)
        {
            _cadenaConexion = configuracion.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
        }

        private SqlCommand CrearComando(SqlConnection conexion, string procedimiento) =>
            new(procedimiento, conexion) { CommandType = CommandType.StoredProcedure };

        // ===== Pantalla "Ver lugar" =====
        public async Task<LugarDetalleViewModel?> ObtenerAsync(int lugarId, string usuarioId, string filtro)
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Lugar_Obtener");
            comando.Parameters.Add("@LugarId", SqlDbType.Int).Value = lugarId;
            comando.Parameters.Add("@UsuarioId", SqlDbType.NVarChar, 450).Value = usuarioId;
            comando.Parameters.Add("@Filtro", SqlDbType.NVarChar, 10).Value = filtro;

            await conexion.OpenAsync();
            using var lector = await comando.ExecuteReaderAsync();

            if (!await lector.ReadAsync()) return null;

            var lugar = new LugarDetalleViewModel
            {
                LugarId = lector.GetInt32(lector.GetOrdinal("LugarId")),
                Nombre = lector.GetString(lector.GetOrdinal("Nombre")),
                Provincia = lector.GetString(lector.GetOrdinal("Provincia")),
                Latitud = lector.GetDecimal(lector.GetOrdinal("Latitud")),
                Longitud = lector.GetDecimal(lector.GetOrdinal("Longitud")),
                EsFavorito = lector.GetBoolean(lector.GetOrdinal("EsFavorito")),
                CantidadFavoritos = lector.GetInt32(lector.GetOrdinal("CantidadFavoritos")),
                Filtro = filtro
            };

            // Etiquetas del lugar
            await lector.NextResultAsync();
            while (await lector.ReadAsync())
            {
                lugar.Etiquetas.Add(new EtiquetaViewModel
                {
                    EtiquetaId = lector.GetInt32(lector.GetOrdinal("EtiquetaId")),
                    Nombre = lector.GetString(lector.GetOrdinal("Nombre")),
                    Icono = lector.GetString(lector.GetOrdinal("Icono"))
                });
            }

            // Aventuras
            await lector.NextResultAsync();
            var aventurasPorId = new Dictionary<int, AventuraLugarViewModel>();

            while (await lector.ReadAsync())
            {
                var aventura = new AventuraLugarViewModel
                {
                    AventuraId = lector.GetInt32(lector.GetOrdinal("AventuraId")),
                    Descripcion = lector["Descripcion"] as string,
                    FechaPublicacion = lector.GetDateTime(lector.GetOrdinal("FechaPublicacion")),
                    NombreUsuario = lector.GetString(lector.GetOrdinal("NombreUsuario")),
                    NombreMostrar = lector.GetString(lector.GetOrdinal("NombreMostrar")),
                    AvatarUrl = lector["AvatarUrl"] as string,
                    EsMia = lector.GetBoolean(lector.GetOrdinal("EsMia")),
                    CantidadLikes = lector.GetInt32(lector.GetOrdinal("CantidadLikes")),
                    MeGusta = lector.GetBoolean(lector.GetOrdinal("MeGusta")),
                    CantidadComentarios = lector.GetInt32(lector.GetOrdinal("CantidadComentarios"))
                };

                lugar.Aventuras.Add(aventura);
                aventurasPorId[aventura.AventuraId] = aventura;
            }

            // Fotos de las aventuras
            await lector.NextResultAsync();
            while (await lector.ReadAsync())
            {
                var id = lector.GetInt32(lector.GetOrdinal("AventuraId"));

                if (aventurasPorId.TryGetValue(id, out var aventura))
                {
                    aventura.Fotos.Add(new FotoViewModel
                    {
                        FotoId = lector.GetInt32(lector.GetOrdinal("FotoId")),
                        Url = lector.GetString(lector.GetOrdinal("Url"))
                    });
                }
            }

            // Etiquetas personales
            await lector.NextResultAsync();
            while (await lector.ReadAsync())
            {
                var id = lector.GetInt32(lector.GetOrdinal("AventuraId"));

                if (aventurasPorId.TryGetValue(id, out var aventura))
                {
                    aventura.Etiquetas.Add(new EtiquetaViewModel
                    {
                        Nombre = lector.GetString(lector.GetOrdinal("Nombre")),
                        Icono = lector.GetString(lector.GetOrdinal("Icono"))
                    });
                }
            }

            return lugar;
        }

        // ===== Alternar Like =====
        public async Task<LikeResultadoViewModel?> AlternarLikeAsync(string usuarioId, int aventuraId)
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Like_Alternar");
            comando.Parameters.Add("@UsuarioId", SqlDbType.NVarChar, 450).Value = usuarioId;
            comando.Parameters.Add("@AventuraId", SqlDbType.Int).Value = aventuraId;

            await conexion.OpenAsync();
            using var lector = await comando.ExecuteReaderAsync();

            if (!await lector.ReadAsync()) return null;

            return new LikeResultadoViewModel
            {
                MeGusta = lector.GetBoolean(lector.GetOrdinal("MeGusta")),
                CantidadLikes = lector.GetInt32(lector.GetOrdinal("CantidadLikes"))
            };
        }

        // ===== Alternar Favorito =====
        public async Task<FavoritoResultadoViewModel?> AlternarFavoritoAsync(string usuarioId, int lugarId)
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_LugarFavorito_Alternar");
            comando.Parameters.Add("@UsuarioId", SqlDbType.NVarChar, 450).Value = usuarioId;
            comando.Parameters.Add("@LugarId", SqlDbType.Int).Value = lugarId;

            await conexion.OpenAsync();
            using var lector = await comando.ExecuteReaderAsync();

            if (!await lector.ReadAsync()) return null;

            return new FavoritoResultadoViewModel
            {
                EsFavorito = lector.GetBoolean(lector.GetOrdinal("EsFavorito")),
                CantidadFavoritos = lector.GetInt32(lector.GetOrdinal("CantidadFavoritos"))
            };
        }

        // ===== Agregar aventura =====
        public async Task<int> AgregarAventuraAsync(string usuarioId, AgregarAventuraViewModel datos)
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Aventura_Crear");

            comando.Parameters.Add("@UsuarioId", SqlDbType.NVarChar, 450).Value = usuarioId;
            comando.Parameters.Add("@LugarId", SqlDbType.Int).Value = datos.LugarId;

            comando.Parameters.Add("@ProvinciaId", SqlDbType.Int).Value = 0;
            comando.Parameters.Add("@Titulo", SqlDbType.NVarChar, 100).Value = "";

            var latitud = comando.Parameters.Add("@Latitud", SqlDbType.Decimal);
            latitud.Precision = 9;
            latitud.Scale = 6;
            latitud.Value = 0m;

            var longitud = comando.Parameters.Add("@Longitud", SqlDbType.Decimal);
            longitud.Precision = 9;
            longitud.Scale = 6;
            longitud.Value = 0m;

            comando.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 1000).Value =
                string.IsNullOrWhiteSpace(datos.Descripcion)
                    ? DBNull.Value
                    : datos.Descripcion.Trim();

            comando.Parameters.Add("@EsPublica", SqlDbType.Bit).Value = datos.EsPublica;

            comando.Parameters.Add("@EtiquetaIds", SqlDbType.NVarChar, 200).Value =
                datos.EtiquetaIds.Count == 0
                    ? DBNull.Value
                    : string.Join(",", datos.EtiquetaIds.Distinct());

            await conexion.OpenAsync();
            return Convert.ToInt32(await comando.ExecuteScalarAsync());
        }

        // ===== Agregar foto =====
        public async Task AgregarFotoAsync(int aventuraId, string usuarioId, string url, int orden)
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Foto_Agregar");

            comando.Parameters.Add("@AventuraId", SqlDbType.Int).Value = aventuraId;
            comando.Parameters.Add("@UsuarioId", SqlDbType.NVarChar, 450).Value = usuarioId;
            comando.Parameters.Add("@Url", SqlDbType.NVarChar, 400).Value = url;
            comando.Parameters.Add("@Orden", SqlDbType.Int).Value = orden;

            await conexion.OpenAsync();
            await comando.ExecuteNonQueryAsync();
        }

        // ===== Eliminar foto =====
        public async Task<string?> EliminarFotoAsync(int fotoId, string usuarioId)
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Foto_Eliminar");
            comando.Parameters.Add("@FotoId", SqlDbType.Int).Value = fotoId;
            comando.Parameters.Add("@UsuarioId", SqlDbType.NVarChar, 450).Value = usuarioId;

            await conexion.OpenAsync();
            var resultado = await comando.ExecuteScalarAsync();
            return resultado as string;
        }

        // ===== Listar favoritos =====
        public async Task<List<LugarFavoritoViewModel>> ListarFavoritosAsync(string usuarioId)
        {
            var favoritos = new List<LugarFavoritoViewModel>();

            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_LugarFavoritos_Listar");

            comando.Parameters.Add("@UsuarioId", SqlDbType.NVarChar, 450).Value = usuarioId;

            await conexion.OpenAsync();
            using var lector = await comando.ExecuteReaderAsync();

            while (await lector.ReadAsync())
            {
                favoritos.Add(new LugarFavoritoViewModel
                {
                    LugarId = lector.GetInt32(lector.GetOrdinal("LugarId")),
                    Nombre = lector.GetString(lector.GetOrdinal("Nombre")),
                    Provincia = lector.GetString(lector.GetOrdinal("Provincia")),
                    Estado = lector.GetString(lector.GetOrdinal("Estado")),
                    FechaAgregado = lector.GetDateTime(lector.GetOrdinal("FechaAgregado"))
                });
            }

            return favoritos;
        }

        // ===== Listar publicaciones del Feed =====
        public async Task<List<PublicacionFeedViewModel>> ListarFeedAsync(string usuarioId)
        {
            var publicaciones = new List<PublicacionFeedViewModel>();

            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Feed_Listar");

            comando.Parameters.Add("@UsuarioId", SqlDbType.NVarChar, 450).Value = usuarioId;

            await conexion.OpenAsync();
            using var lector = await comando.ExecuteReaderAsync();

            // Publicaciones
            while (await lector.ReadAsync())
            {
                publicaciones.Add(new PublicacionFeedViewModel
                {
                    AventuraId = lector.GetInt32(lector.GetOrdinal("AventuraId")),
                    NombreUsuario = lector.GetString(lector.GetOrdinal("NombreUsuario")),
                    AvatarUrl = lector["AvatarUrl"] as string,
                    Lugar = lector.GetString(lector.GetOrdinal("Lugar")),
                    FotoUrl = lector["FotoUrl"] as string,
                    Descripcion = lector.GetString(lector.GetOrdinal("Descripcion")),
                    FechaPublicacion = lector.GetDateTime(lector.GetOrdinal("FechaPublicacion")),
                    CantidadLikes = lector.GetInt32(lector.GetOrdinal("CantidadLikes")),
                    LeDiLike = lector.GetBoolean(lector.GetOrdinal("LeDiLike"))
                });
            }

            // Etiquetas
            await lector.NextResultAsync();

            while (await lector.ReadAsync())
            {
                int aventuraId = lector.GetInt32(lector.GetOrdinal("AventuraId"));

                var publicacion = publicaciones.FirstOrDefault(
                    p => p.AventuraId == aventuraId
                );

                if (publicacion != null)
                {
                    publicacion.Etiquetas.Add(new EtiquetaViewModel
                    {
                        EtiquetaId = lector.GetInt32(lector.GetOrdinal("EtiquetaId")),
                        Nombre = lector.GetString(lector.GetOrdinal("Nombre")),
                        Icono = lector.GetString(lector.GetOrdinal("Icono"))
                    });
                }
            }

            return publicaciones;
        }


        // ===== Listar comentarios de una aventura =====
        public async Task<List<ComentarioFeedViewModel>> ListarComentariosAsync(
            string usuarioId, int aventuraId)
        {
            var comentarios = new List<ComentarioFeedViewModel>();

            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Comentarios_Listar");

            comando.Parameters.Add("@UsuarioId", SqlDbType.NVarChar, 450).Value = usuarioId;
            comando.Parameters.Add("@AventuraId", SqlDbType.Int).Value = aventuraId;

            await conexion.OpenAsync();
            using var lector = await comando.ExecuteReaderAsync();

            while (await lector.ReadAsync())
            {
                comentarios.Add(new ComentarioFeedViewModel
                {
                    ComentarioId = lector.GetInt32(lector.GetOrdinal("ComentarioId")),
                    AventuraId = lector.GetInt32(lector.GetOrdinal("AventuraId")),
                    NombreUsuario = lector.GetString(lector.GetOrdinal("NombreUsuario")),
                    Texto = lector.GetString(lector.GetOrdinal("Texto")),
                    Fecha = lector.GetDateTime(lector.GetOrdinal("Fecha"))
                });
            }

            return comentarios;
        }

        // Agregar comentario a una aventura 
        public async Task AgregarComentarioAsync(
            string usuarioId, int aventuraId, string texto)
        {
            if (string.IsNullOrWhiteSpace(texto) || texto.Trim().Length > 500)
            {
                throw new ArgumentException(
                    "El comentario debe contener entre 1 y 500 caracteres.",
                    nameof(texto));
            }

            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Comentario_Agregar");

            comando.Parameters.Add("@UsuarioId", SqlDbType.NVarChar, 450).Value = usuarioId;
            comando.Parameters.Add("@AventuraId", SqlDbType.Int).Value = aventuraId;
            comando.Parameters.Add("@Texto", SqlDbType.NVarChar, 500).Value = texto.Trim();

            await conexion.OpenAsync();
            await comando.ExecuteNonQueryAsync();
        }

    }
}
