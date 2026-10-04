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

        // ===== Pantalla "Ver lugar" (sp_Lugar_Obtener devuelve 5 resultados) =====
        public async Task<LugarDetalleViewModel?> ObtenerAsync(int lugarId, string usuarioId, string filtro)
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Lugar_Obtener");
            comando.Parameters.Add("@LugarId", SqlDbType.Int).Value = lugarId;
            comando.Parameters.Add("@UsuarioId", SqlDbType.NVarChar, 450).Value = usuarioId;
            comando.Parameters.Add("@Filtro", SqlDbType.NVarChar, 10).Value = filtro;

            await conexion.OpenAsync();
            using var lector = await comando.ExecuteReaderAsync();

            // 1) El lugar. Si viene vacío, no existe o no se puede ver
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

            // 2) Etiquetas fijas del lugar
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

            // 3) Aventuras
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

            // 4) Fotos, se pegan a su aventura
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

            // 5) Etiquetas personales, se pegan a su aventura
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

        // ===== Like: si ya le di, lo quita; si no, lo pone =====
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

        // ===== Favorito: si ya está, lo quita; si no, lo agrega =====
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

        // ===== Agregar mi aventura a un lugar que ya existe =====
        public async Task<int> AgregarAventuraAsync(string usuarioId, AgregarAventuraViewModel datos)
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Aventura_Crear");

            comando.Parameters.Add("@UsuarioId", SqlDbType.NVarChar, 450).Value = usuarioId;
            comando.Parameters.Add("@LugarId", SqlDbType.Int).Value = datos.LugarId;

            // Con @LugarId, el SP toma nombre, provincia y ubicación del lugar.
            // Estos cuatro se mandan solo porque el SP los pide.
            comando.Parameters.Add("@ProvinciaId", SqlDbType.Int).Value = 0;
            comando.Parameters.Add("@Titulo", SqlDbType.NVarChar, 100).Value = "";
            var latitud = comando.Parameters.Add("@Latitud", SqlDbType.Decimal);
            latitud.Precision = 9; latitud.Scale = 6; latitud.Value = 0m;
            var longitud = comando.Parameters.Add("@Longitud", SqlDbType.Decimal);
            longitud.Precision = 9; longitud.Scale = 6; longitud.Value = 0m;

            comando.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 1000).Value =
                string.IsNullOrWhiteSpace(datos.Descripcion) ? DBNull.Value : datos.Descripcion.Trim();
            comando.Parameters.Add("@EsPublica", SqlDbType.Bit).Value = datos.EsPublica;
            comando.Parameters.Add("@EtiquetaIds", SqlDbType.NVarChar, 200).Value =
                datos.EtiquetaIds.Count == 0 ? DBNull.Value : string.Join(",", datos.EtiquetaIds.Distinct());

            await conexion.OpenAsync();
            return Convert.ToInt32(await comando.ExecuteScalarAsync());
        }

        // ===== Guardar una foto de mi aventura =====
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

        // ===== Borrar una foto de mi aventura =====
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
    }
}