using System.Data;
using Mapiticos.Models.ViewModels;
using Microsoft.Data.SqlClient;

namespace Mapiticos.Repositorios
{
    public class NotificacionRepositorio : INotificacionRepositorio
    {
        private readonly string _cadenaConexion;

        public NotificacionRepositorio(IConfiguration configuracion)
        {
            _cadenaConexion = configuracion.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
        }

        private SqlCommand CrearComando(SqlConnection conexion, string procedimiento) =>
            new(procedimiento, conexion) { CommandType = CommandType.StoredProcedure };

        // ===== Mis avisos =====
        public async Task<List<NotificacionViewModel>> ListarAsync(string usuarioId)
        {
            var avisos = new List<NotificacionViewModel>();

            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Notificaciones_Listar");
            comando.Parameters.Add("@UsuarioId", SqlDbType.NVarChar, 450).Value = usuarioId;

            await conexion.OpenAsync();
            using var lector = await comando.ExecuteReaderAsync();

            while (await lector.ReadAsync())
            {
                avisos.Add(new NotificacionViewModel
                {
                    NotificacionId = lector.GetInt32(lector.GetOrdinal("NotificacionId")),
                    Tipo = lector.GetString(lector.GetOrdinal("Tipo")),
                    Leida = lector.GetBoolean(lector.GetOrdinal("Leida")),
                    Fecha = lector.GetDateTime(lector.GetOrdinal("Fecha")),
                    NombreUsuario = lector.GetString(lector.GetOrdinal("NombreUsuario")),
                    NombreMostrar = lector.GetString(lector.GetOrdinal("NombreMostrar")),
                    AvatarUrl = lector["AvatarUrl"] as string,
                    LugarId = lector["LugarId"] as int?,
                    Lugar = lector["Lugar"] as string
                });
            }

            return avisos;
        }

        // ===== Marcar como leídos =====
        public async Task MarcarLeidasAsync(string usuarioId)
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Notificaciones_MarcarLeidas");
            comando.Parameters.Add("@UsuarioId", SqlDbType.NVarChar, 450).Value = usuarioId;

            await conexion.OpenAsync();
            await comando.ExecuteNonQueryAsync();
        }
    }
}
