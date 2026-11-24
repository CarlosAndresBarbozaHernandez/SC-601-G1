using System.Data;
using Mapiticos.Models.ViewModels;
using Microsoft.Data.SqlClient;

namespace Mapiticos.Repositorios
{
    public class PerfilRepositorio : IPerfilRepositorio
    {
        private readonly string _cadenaConexion;

        // Códigos de error de SQL Server cuando se viola un índice único
        private const int ErrorDuplicado1 = 2601;
        private const int ErrorDuplicado2 = 2627;

        public PerfilRepositorio(IConfiguration configuracion)
        {
            _cadenaConexion = configuracion.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
        }

        private SqlCommand CrearComando(SqlConnection conexion, string procedimiento) =>
            new(procedimiento, conexion) { CommandType = CommandType.StoredProcedure };

        // ===== Perfil completo =====
        public async Task<PerfilViewModel?> ObtenerPerfilAsync(string usuarioId)
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Perfil_Obtener");
            comando.Parameters.Add("@UsuarioId", SqlDbType.NVarChar, 450).Value = usuarioId;

            await conexion.OpenAsync();
            using var lector = await comando.ExecuteReaderAsync();

            if (!await lector.ReadAsync()) return null;

            var perfil = new PerfilViewModel
            {
                NombreUsuario = lector.GetString(lector.GetOrdinal("NombreUsuario")),
                NombreMostrar = lector.GetString(lector.GetOrdinal("NombreMostrar")),
                Biografia = lector["Biografia"] as string,
                AvatarUrl = lector["AvatarUrl"] as string,
                EsPrivada = lector.GetBoolean(lector.GetOrdinal("EsPrivada")),
                CantidadSeguidores = lector.GetInt32(lector.GetOrdinal("CantidadSeguidores")),
                CantidadSiguiendo = lector.GetInt32(lector.GetOrdinal("CantidadSiguiendo"))
            };

            await lector.NextResultAsync();
            var aventurasPorId = new Dictionary<int, AventuraMiniaturaViewModel>();
            while (await lector.ReadAsync())
            {
                var aventura = new AventuraMiniaturaViewModel
                {
                    AventuraId = lector.GetInt32(lector.GetOrdinal("AventuraId")),
                    Titulo = lector.GetString(lector.GetOrdinal("Titulo")),
                    FotoUrl = lector["FotoUrl"] as string
                };
                perfil.Aventuras.Add(aventura);
                aventurasPorId[aventura.AventuraId] = aventura;
            }

            await lector.NextResultAsync();
            while (await lector.ReadAsync())
            {
                var aventuraId = lector.GetInt32(lector.GetOrdinal("AventuraId"));
                if (aventurasPorId.TryGetValue(aventuraId, out var aventura))
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

        // ===== Datos para Editar perfil =====
        public async Task<EditarPerfilViewModel?> ObtenerParaEditarAsync(string usuarioId)
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Perfil_ObtenerParaEditar");
            comando.Parameters.Add("@UsuarioId", SqlDbType.NVarChar, 450).Value = usuarioId;

            await conexion.OpenAsync();
            using var lector = await comando.ExecuteReaderAsync();

            if (!await lector.ReadAsync()) return null;

            return new EditarPerfilViewModel
            {
                NombreUsuario = lector.GetString(lector.GetOrdinal("NombreUsuario")),
                NombreMostrar = lector.GetString(lector.GetOrdinal("NombreMostrar")),
                Biografia = lector["Biografia"] as string,
                AvatarUrl = lector["AvatarUrl"] as string,
                EsPrivada = lector.GetBoolean(lector.GetOrdinal("EsPrivada"))
            };
        }

        // ===== Guardar perfil =====
        public async Task<bool> GuardarAsync(string usuarioId, EditarPerfilViewModel datos)
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Perfil_Guardar");

            comando.Parameters.Add("@UsuarioId", SqlDbType.NVarChar, 450).Value = usuarioId;
            comando.Parameters.Add("@NombreUsuario", SqlDbType.NVarChar, 30).Value = datos.NombreUsuario;
            comando.Parameters.Add("@NombreMostrar", SqlDbType.NVarChar, 100).Value = datos.NombreMostrar.Trim();
            comando.Parameters.Add("@Biografia", SqlDbType.NVarChar, 300).Value =
                string.IsNullOrWhiteSpace(datos.Biografia) ? DBNull.Value : datos.Biografia.Trim();
            comando.Parameters.Add("@AvatarUrl", SqlDbType.NVarChar, 400).Value =
                string.IsNullOrWhiteSpace(datos.AvatarUrl) ? DBNull.Value : datos.AvatarUrl;
            comando.Parameters.Add("@EsPrivada", SqlDbType.Bit).Value = datos.EsPrivada;

            await conexion.OpenAsync();
            try
            {
                await comando.ExecuteNonQueryAsync();
                return true;
            }
            catch (SqlException ex) when (ex.Number == ErrorDuplicado1 || ex.Number == ErrorDuplicado2)
            {
                return false; // otra persona ya tiene ese @usuario
            }
        }

        // ===== ¿Ya eligió su @usuario? =====
        public async Task<string?> ObtenerNombreUsuarioAsync(string usuarioId)
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Perfil_ObtenerNombreUsuario");
            comando.Parameters.Add("@UsuarioId", SqlDbType.NVarChar, 450).Value = usuarioId;

            await conexion.OpenAsync();
            var resultado = await comando.ExecuteScalarAsync();
            return resultado as string;
        }

        // ===== ¿Está disponible? =====
        public async Task<bool> NombreUsuarioDisponibleAsync(string nombreUsuario, string usuarioId)
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Perfil_NombreUsuarioDisponible");
            comando.Parameters.Add("@NombreUsuario", SqlDbType.NVarChar, 30).Value = nombreUsuario;
            comando.Parameters.Add("@UsuarioId", SqlDbType.NVarChar, 450).Value = usuarioId;

            await conexion.OpenAsync();
            var resultado = await comando.ExecuteScalarAsync();
            return resultado is bool disponible && disponible;
        }

        // ===== Asignar el @usuario =====
        public async Task<bool> AsignarNombreUsuarioAsync(string usuarioId, string nombreUsuario)
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Perfil_AsignarNombreUsuario");
            comando.Parameters.Add("@UsuarioId", SqlDbType.NVarChar, 450).Value = usuarioId;
            comando.Parameters.Add("@NombreUsuario", SqlDbType.NVarChar, 30).Value = nombreUsuario;

            await conexion.OpenAsync();
            try
            {
                await comando.ExecuteNonQueryAsync();
                return true;
            }
            catch (SqlException ex) when (ex.Number == ErrorDuplicado1 || ex.Number == ErrorDuplicado2)
            {
                return false;
            }
        }

        // ===== Modo (tema) de la app =====
        public async Task<string?> ObtenerTemaAsync(string usuarioId)
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Perfil_ObtenerTema");
            comando.Parameters.Add("@UsuarioId", SqlDbType.NVarChar, 450).Value = usuarioId;

            await conexion.OpenAsync();
            return await comando.ExecuteScalarAsync() as string;
        }

        public async Task GuardarTemaAsync(string usuarioId, string tema)
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Perfil_GuardarTema");
            comando.Parameters.Add("@UsuarioId", SqlDbType.NVarChar, 450).Value = usuarioId;
            comando.Parameters.Add("@Tema", SqlDbType.NVarChar, 20).Value = tema;

            await conexion.OpenAsync();
            await comando.ExecuteNonQueryAsync();
        }
    }
}