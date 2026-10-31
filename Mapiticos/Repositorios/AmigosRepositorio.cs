using System.Data;
using Mapiticos.Models.ViewModels;
using Microsoft.Data.SqlClient;

namespace Mapiticos.Repositorios
{
    public class AmigosRepositorio : IAmigosRepositorio
    {
        private readonly string _cadenaConexion;

        public AmigosRepositorio(IConfiguration configuracion)
        {
            _cadenaConexion = configuracion.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
        }

        private SqlCommand CrearComando(SqlConnection conexion, string procedimiento) =>
            new(procedimiento, conexion) { CommandType = CommandType.StoredProcedure };

        private static bool EsErrorDeNegocio(SqlException ex) => ex.Number >= 52000 && ex.Number < 53000;

        // ===== Buscar =====
        public async Task<List<UsuarioBusquedaViewModel>> BuscarAsync(string usuarioId, string texto)
        {
            var resultados = new List<UsuarioBusquedaViewModel>();

            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Usuarios_Buscar");
            comando.Parameters.Add("@UsuarioId", SqlDbType.NVarChar, 450).Value = usuarioId;
            comando.Parameters.Add("@Texto", SqlDbType.NVarChar, 100).Value = texto;

            await conexion.OpenAsync();
            using var lector = await comando.ExecuteReaderAsync();

            while (await lector.ReadAsync())
            {
                resultados.Add(new UsuarioBusquedaViewModel
                {
                    NombreUsuario = lector.GetString(lector.GetOrdinal("NombreUsuario")),
                    NombreMostrar = lector.GetString(lector.GetOrdinal("NombreMostrar")),
                    AvatarUrl = lector["AvatarUrl"] as string,
                    EsPrivada = lector.GetBoolean(lector.GetOrdinal("EsPrivada")),
                    EstadoSeguimiento = lector.GetString(lector.GetOrdinal("EstadoSeguimiento")),
                    MeSigue = lector.GetBoolean(lector.GetOrdinal("MeSigue"))
                });
            }

            return resultados;
        }

        // ===== Seguir / dejar de seguir =====
        public async Task<string> AlternarSeguimientoAsync(string usuarioId, string nombreUsuario)
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Seguimiento_Alternar");
            comando.Parameters.Add("@UsuarioId", SqlDbType.NVarChar, 450).Value = usuarioId;
            comando.Parameters.Add("@NombreUsuario", SqlDbType.NVarChar, 30).Value = nombreUsuario;

            try
            {
                await conexion.OpenAsync();
                return (await comando.ExecuteScalarAsync()) as string ?? EstadosSeguimiento.Ninguno;
            }
            catch (SqlException ex) when (EsErrorDeNegocio(ex))
            {
                throw new AmigosException(ex.Message, ex);
            }
        }

        // ===== Solicitudes que me llegaron =====
        public async Task<List<SolicitudSeguimientoViewModel>> ListarSolicitudesAsync(string usuarioId)
        {
            var solicitudes = new List<SolicitudSeguimientoViewModel>();

            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Solicitudes_Listar");
            comando.Parameters.Add("@UsuarioId", SqlDbType.NVarChar, 450).Value = usuarioId;

            await conexion.OpenAsync();
            using var lector = await comando.ExecuteReaderAsync();

            while (await lector.ReadAsync())
            {
                solicitudes.Add(new SolicitudSeguimientoViewModel
                {
                    NombreUsuario = lector.GetString(lector.GetOrdinal("NombreUsuario")),
                    NombreMostrar = lector.GetString(lector.GetOrdinal("NombreMostrar")),
                    AvatarUrl = lector["AvatarUrl"] as string,
                    FechaSolicitud = lector.GetDateTime(lector.GetOrdinal("FechaSolicitud"))
                });
            }

            return solicitudes;
        }

        // ===== Aceptar / rechazar =====
        public async Task ResponderSolicitudAsync(string usuarioId, string nombreUsuario, bool acepta)
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Solicitud_Responder");
            comando.Parameters.Add("@UsuarioId", SqlDbType.NVarChar, 450).Value = usuarioId;
            comando.Parameters.Add("@NombreUsuario", SqlDbType.NVarChar, 30).Value = nombreUsuario;
            comando.Parameters.Add("@Acepta", SqlDbType.Bit).Value = acepta;

            try
            {
                await conexion.OpenAsync();
                await comando.ExecuteNonQueryAsync();
            }
            catch (SqlException ex) when (EsErrorDeNegocio(ex))
            {
                throw new AmigosException(ex.Message, ex);
            }
        }
    

        // ===== Perfil de otra persona =====
        public async Task<PerfilPublicoViewModel?> ObtenerPerfilPublicoAsync(string visitanteId, string nombreUsuario)
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Perfil_ObtenerPublico");
            comando.Parameters.Add("@VisitanteId", SqlDbType.NVarChar, 450).Value = visitanteId;
            comando.Parameters.Add("@NombreUsuario", SqlDbType.NVarChar, 30).Value = nombreUsuario;

            await conexion.OpenAsync();
            using var lector = await comando.ExecuteReaderAsync();

            if (!await lector.ReadAsync()) return null;

            var perfil = new PerfilPublicoViewModel
            {
                NombreUsuario = lector.GetString(lector.GetOrdinal("NombreUsuario")),
                NombreMostrar = lector.GetString(lector.GetOrdinal("NombreMostrar")),
                Biografia = lector["Biografia"] as string,
                AvatarUrl = lector["AvatarUrl"] as string,
                EsPrivada = lector.GetBoolean(lector.GetOrdinal("EsPrivada")),
                CantidadSeguidores = lector.GetInt32(lector.GetOrdinal("CantidadSeguidores")),
                CantidadSiguiendo = lector.GetInt32(lector.GetOrdinal("CantidadSiguiendo")),
                CantidadAventurasVisibles = lector.GetInt32(lector.GetOrdinal("CantidadAventurasVisibles")),
                EsMio = lector.GetBoolean(lector.GetOrdinal("EsMio")),
                EstadoSeguimiento = lector.GetString(lector.GetOrdinal("EstadoSeguimiento")),
                MeSigue = lector.GetBoolean(lector.GetOrdinal("MeSigue")),
                PuedeVer = lector.GetBoolean(lector.GetOrdinal("PuedeVer"))
            };

            // Aventuras
            await lector.NextResultAsync();
            var porId = new Dictionary<int, AventuraMiniaturaViewModel>();
            while (await lector.ReadAsync())
            {
                var aventura = new AventuraMiniaturaViewModel
                {
                    AventuraId = lector.GetInt32(lector.GetOrdinal("AventuraId")),
                    Titulo = lector.GetString(lector.GetOrdinal("Titulo")),
                    FotoUrl = lector["FotoUrl"] as string
                };
                perfil.Aventuras.Add(aventura);
                porId[aventura.AventuraId] = aventura;
            }

            // Etiquetas
            await lector.NextResultAsync();
            while (await lector.ReadAsync())
            {
                var id = lector.GetInt32(lector.GetOrdinal("AventuraId"));
                if (porId.TryGetValue(id, out var aventura))
                {
                    aventura.Etiquetas.Add(new EtiquetaViewModel
                    {
                        Nombre = lector.GetString(lector.GetOrdinal("Nombre")),
                        Icono = lector.GetString(lector.GetOrdinal("Icono"))
                    });
                }
            }

            return perfil;
        }
    }
}
