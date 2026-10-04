using System.Data;
using Mapiticos.Models.ViewModels;
using Microsoft.Data.SqlClient;

namespace Mapiticos.Repositorios
{
    public class MapaRepositorio : IMapaRepositorio
    {
        private readonly string _cadenaConexion;

        public MapaRepositorio(IConfiguration configuracion)
        {
            _cadenaConexion = configuracion.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
        }

        private SqlCommand CrearComando(SqlConnection conexion, string procedimiento) =>
            new(procedimiento, conexion) { CommandType = CommandType.StoredProcedure };

        // ===== Pines del mapa: un pin por lugar =====
        public async Task<List<LugarMapaViewModel>> ListarLugaresAsync(string usuarioId)
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Mapa_ListarLugares");
            comando.Parameters.Add("@UsuarioId", SqlDbType.NVarChar, 450).Value = usuarioId;

            await conexion.OpenAsync();
            using var lector = await comando.ExecuteReaderAsync();

            // Resultado 1: los lugares
            var lugares = new Dictionary<int, LugarMapaViewModel>();
            while (await lector.ReadAsync())
            {
                var lugar = new LugarMapaViewModel
                {
                    LugarId = lector.GetInt32(lector.GetOrdinal("LugarId")),
                    Nombre = lector.GetString(lector.GetOrdinal("Nombre")),
                    Provincia = lector.GetString(lector.GetOrdinal("Provincia")),
                    Latitud = lector.GetDecimal(lector.GetOrdinal("Latitud")),
                    Longitud = lector.GetDecimal(lector.GetOrdinal("Longitud")),
                    CantidadAventuras = lector.GetInt32(lector.GetOrdinal("CantidadAventuras")),
                    TengoAventura = lector.GetBoolean(lector.GetOrdinal("TengoAventura")),
                    EsFavorito = lector.GetBoolean(lector.GetOrdinal("EsFavorito"))
                };
                lugares[lugar.LugarId] = lugar;
            }

            // Resultado 2: etiquetas fijas, se pegan a su lugar
            await lector.NextResultAsync();
            while (await lector.ReadAsync())
            {
                var id = lector.GetInt32(lector.GetOrdinal("LugarId"));
                if (lugares.TryGetValue(id, out var lugar))
                {
                    lugar.Etiquetas.Add(new EtiquetaViewModel
                    {
                        EtiquetaId = lector.GetInt32(lector.GetOrdinal("EtiquetaId")),
                        Nombre = lector.GetString(lector.GetOrdinal("Nombre")),
                        Icono = lector.GetString(lector.GetOrdinal("Icono"))
                    });
                }
            }

            return lugares.Values.ToList();
        }

        // ===== En qué lugar está una aventura (para "Ver en el mapa") =====
        public async Task<int?> ObtenerLugarDeAventuraAsync(int aventuraId)
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Aventura_ObtenerLugar");
            comando.Parameters.Add("@AventuraId", SqlDbType.Int).Value = aventuraId;

            await conexion.OpenAsync();
            var resultado = await comando.ExecuteScalarAsync();
            return resultado is int lugarId ? lugarId : null;
        }

        public async Task<List<ProvinciaViewModel>> ListarProvinciasAsync()
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Provincias_Listar");
            await conexion.OpenAsync();
            using var lector = await comando.ExecuteReaderAsync();

            var lista = new List<ProvinciaViewModel>();
            while (await lector.ReadAsync())
            {
                lista.Add(new ProvinciaViewModel
                {
                    ProvinciaId = lector.GetInt32(lector.GetOrdinal("ProvinciaId")),
                    Nombre = lector.GetString(lector.GetOrdinal("Nombre"))
                });
            }
            return lista;
        }

        public async Task<List<EtiquetaViewModel>> ListarEtiquetasAsync()
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Etiquetas_Listar");
            await conexion.OpenAsync();
            using var lector = await comando.ExecuteReaderAsync();

            var lista = new List<EtiquetaViewModel>();
            while (await lector.ReadAsync())
            {
                lista.Add(new EtiquetaViewModel
                {
                    EtiquetaId = lector.GetInt32(lector.GetOrdinal("EtiquetaId")),
                    Nombre = lector.GetString(lector.GetOrdinal("Nombre")),
                    Icono = lector.GetString(lector.GetOrdinal("Icono"))
                });
            }
            return lista;
        }

        public async Task<int> CrearPuntoAsync(string usuarioId, CrearPuntoViewModel datos)
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Aventura_Crear");

            comando.Parameters.Add("@UsuarioId", SqlDbType.NVarChar, 450).Value = usuarioId;
            comando.Parameters.Add("@ProvinciaId", SqlDbType.Int).Value = datos.ProvinciaId;
            comando.Parameters.Add("@Titulo", SqlDbType.NVarChar, 100).Value = datos.Titulo.Trim();
            comando.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 1000).Value =
                string.IsNullOrWhiteSpace(datos.Descripcion) ? DBNull.Value : datos.Descripcion.Trim();

            // DECIMAL(9,6) igual que la columna, para no perder precisión
            var latitud = comando.Parameters.Add("@Latitud", SqlDbType.Decimal);
            latitud.Precision = 9; latitud.Scale = 6; latitud.Value = datos.Latitud;
            var longitud = comando.Parameters.Add("@Longitud", SqlDbType.Decimal);
            longitud.Precision = 9; longitud.Scale = 6; longitud.Value = datos.Longitud;

            comando.Parameters.Add("@EsPublica", SqlDbType.Bit).Value = datos.EsPublica;
            comando.Parameters.Add("@EtiquetaIds", SqlDbType.NVarChar, 200).Value =
                datos.EtiquetaIds.Count == 0 ? DBNull.Value : string.Join(",", datos.EtiquetaIds.Distinct());

            await conexion.OpenAsync();
            return Convert.ToInt32(await comando.ExecuteScalarAsync());
        }
    }
}