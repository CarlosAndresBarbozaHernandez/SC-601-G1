using System.Data;
using Mapiticos.Models.ViewModels;
using Microsoft.Data.SqlClient;

namespace Mapiticos.Repositorios
{
    public class EventoRepositorio : IEventoRepositorio
    {
        private readonly string _cadenaConexion;

        public EventoRepositorio(IConfiguration configuracion)
        {
            _cadenaConexion = configuracion.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
        }

        private SqlCommand CrearComando(SqlConnection conexion, string procedimiento) =>
            new(procedimiento, conexion) { CommandType = CommandType.StoredProcedure };

        private static void AgregarUsuario(SqlCommand comando, string usuarioId) =>
            comando.Parameters.Add("@UsuarioId", SqlDbType.NVarChar, 450).Value = usuarioId;

        // 'ana,luis' -> lo que espera el SP. Se limpian vacíos, @ y repetidos.
        private static string UnirInvitados(IEnumerable<string>? invitados) =>
            string.Join(",", (invitados ?? Enumerable.Empty<string>())
                .Select(i => i.Trim().TrimStart('@').ToLowerInvariant())
                .Where(i => i.Length > 0 && !i.Contains(','))
                .Distinct());

        // Ejecuta un SP que no devuelve datos y convierte los THROW 51xxx en EventoException
        private async Task EjecutarAsync(SqlCommand comando)
        {
            try
            {
                await comando.Connection!.OpenAsync();
                await comando.ExecuteNonQueryAsync();
            }
            catch (SqlException ex) when (ex.Number >= 51000 && ex.Number < 52000)
            {
                throw new EventoException(ex.Message, ex);
            }
        }

        // ===== Amigos para invitar =====
        public async Task<List<AmigoViewModel>> ListarAmigosAsync(string usuarioId, int? eventoId = null)
        {
            var amigos = new List<AmigoViewModel>();

            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Amigos_Listar");
            AgregarUsuario(comando, usuarioId);
            comando.Parameters.Add("@EventoId", SqlDbType.Int).Value = (object?)eventoId ?? DBNull.Value;

            await conexion.OpenAsync();
            using var lector = await comando.ExecuteReaderAsync();

            while (await lector.ReadAsync())
            {
                amigos.Add(new AmigoViewModel
                {
                    NombreUsuario = lector.GetString(lector.GetOrdinal("NombreUsuario")),
                    NombreMostrar = lector.GetString(lector.GetOrdinal("NombreMostrar")),
                    AvatarUrl = lector["AvatarUrl"] as string,
                    YaInvitado = lector.GetBoolean(lector.GetOrdinal("YaInvitado"))
                });
            }

            return amigos;
        }

        // ===== Crear =====
        public async Task<int> CrearAsync(string creadorId, CrearEventoViewModel datos)
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Evento_Crear");
            comando.Parameters.Add("@CreadorId", SqlDbType.NVarChar, 450).Value = creadorId;
            comando.Parameters.Add("@LugarId", SqlDbType.Int).Value = datos.LugarId;
            comando.Parameters.Add("@Titulo", SqlDbType.NVarChar, 100).Value = datos.Titulo.Trim();
            comando.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 500).Value =
                string.IsNullOrWhiteSpace(datos.Descripcion) ? DBNull.Value : datos.Descripcion.Trim();
            comando.Parameters.Add("@FechaEvento", SqlDbType.DateTime2).Value = datos.FechaEvento!.Value;
            comando.Parameters.Add("@Invitados", SqlDbType.NVarChar, -1).Value = UnirInvitados(datos.Invitados);

            try
            {
                await conexion.OpenAsync();
                var resultado = await comando.ExecuteScalarAsync();
                return Convert.ToInt32(resultado);
            }
            catch (SqlException ex) when (ex.Number >= 51000 && ex.Number < 52000)
            {
                throw new EventoException(ex.Message, ex);
            }
        }

        // ===== Invitar más amigos =====
        public async Task InvitarAsync(int eventoId, string usuarioId, IEnumerable<string> invitados)
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Evento_Invitar");
            comando.Parameters.Add("@EventoId", SqlDbType.Int).Value = eventoId;
            AgregarUsuario(comando, usuarioId);
            comando.Parameters.Add("@Invitados", SqlDbType.NVarChar, -1).Value = UnirInvitados(invitados);

            await EjecutarAsync(comando);
        }

        // ===== Listar mis eventos =====
        public async Task<List<EventoTarjetaViewModel>> ListarAsync(string usuarioId)
        {
            var eventos = new List<EventoTarjetaViewModel>();
            var porId = new Dictionary<int, EventoTarjetaViewModel>();

            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Eventos_Listar");
            AgregarUsuario(comando, usuarioId);

            await conexion.OpenAsync();
            using var lector = await comando.ExecuteReaderAsync();

            while (await lector.ReadAsync())
            {
                var evento = new EventoTarjetaViewModel();
                LlenarEvento(lector, evento);
                eventos.Add(evento);
                porId[evento.EventoId] = evento;
            }

            await lector.NextResultAsync();
            while (await lector.ReadAsync())
            {
                var id = lector.GetInt32(lector.GetOrdinal("EventoId"));
                if (porId.TryGetValue(id, out var evento))
                    evento.Participantes.Add(LeerParticipante(lector));
            }

            return eventos;
        }

        // ===== Ver un evento =====
        public async Task<EventoDetalleViewModel?> ObtenerAsync(int eventoId, string usuarioId)
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Evento_Obtener");
            comando.Parameters.Add("@EventoId", SqlDbType.Int).Value = eventoId;
            AgregarUsuario(comando, usuarioId);

            await conexion.OpenAsync();
            using var lector = await comando.ExecuteReaderAsync();

            // Sin filas = no existe o no soy participante
            if (!await lector.ReadAsync()) return null;

            var evento = new EventoDetalleViewModel();
            LlenarEvento(lector, evento);
            evento.FechaHecho = lector["FechaHecho"] as DateTime?;

            await lector.NextResultAsync();
            while (await lector.ReadAsync())
                evento.Participantes.Add(LeerParticipante(lector));

            return evento;
        }

        // ===== Aceptar / rechazar =====
        public async Task ResponderAsync(int eventoId, string usuarioId, bool acepta)
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Evento_Responder");
            comando.Parameters.Add("@EventoId", SqlDbType.Int).Value = eventoId;
            AgregarUsuario(comando, usuarioId);
            comando.Parameters.Add("@Acepta", SqlDbType.Bit).Value = acepta;

            await EjecutarAsync(comando);
        }

        // ===== Salir de un evento =====
        public async Task SalirAsync(int eventoId, string usuarioId)
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Evento_Salir");
            comando.Parameters.Add("@EventoId", SqlDbType.Int).Value = eventoId;
            AgregarUsuario(comando, usuarioId);

            await EjecutarAsync(comando);
        }

        // ===== Hecho / Programado =====
        public async Task CambiarEstadoAsync(int eventoId, string usuarioId, bool hecho)
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Evento_CambiarEstado");
            comando.Parameters.Add("@EventoId", SqlDbType.Int).Value = eventoId;
            AgregarUsuario(comando, usuarioId);
            comando.Parameters.Add("@Hecho", SqlDbType.Bit).Value = hecho;

            await EjecutarAsync(comando);
        }

        // ===== Cancelar =====
        public async Task EliminarAsync(int eventoId, string usuarioId)
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Evento_Eliminar");
            comando.Parameters.Add("@EventoId", SqlDbType.Int).Value = eventoId;
            AgregarUsuario(comando, usuarioId);

            await EjecutarAsync(comando);
        }

        // ===== Chat =====
        public async Task<List<MensajeEventoViewModel>> ListarMensajesAsync(int eventoId, string usuarioId, int desdeId)
        {
            var mensajes = new List<MensajeEventoViewModel>();

            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_EventoMensajes_Listar");
            comando.Parameters.Add("@EventoId", SqlDbType.Int).Value = eventoId;
            AgregarUsuario(comando, usuarioId);
            comando.Parameters.Add("@DesdeId", SqlDbType.Int).Value = desdeId;

            await conexion.OpenAsync();
            using var lector = await comando.ExecuteReaderAsync();

            while (await lector.ReadAsync())
            {
                mensajes.Add(new MensajeEventoViewModel
                {
                    MensajeId = lector.GetInt32(lector.GetOrdinal("MensajeId")),
                    NombreUsuario = lector.GetString(lector.GetOrdinal("NombreUsuario")),
                    NombreMostrar = lector.GetString(lector.GetOrdinal("NombreMostrar")),
                    AvatarUrl = lector["AvatarUrl"] as string,
                    Texto = lector.GetString(lector.GetOrdinal("Texto")),
                    Fecha = lector.GetDateTime(lector.GetOrdinal("Fecha")),
                    EsMio = lector.GetBoolean(lector.GetOrdinal("EsMio"))
                });
            }

            return mensajes;
        }

        public async Task AgregarMensajeAsync(int eventoId, string usuarioId, string texto)
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_EventoMensaje_Agregar");
            comando.Parameters.Add("@EventoId", SqlDbType.Int).Value = eventoId;
            AgregarUsuario(comando, usuarioId);
            comando.Parameters.Add("@Texto", SqlDbType.NVarChar, 500).Value = texto;

            await EjecutarAsync(comando);
        }

        // ===== Globito de invitaciones =====
        public async Task<int> ContarPendientesAsync(string usuarioId)
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, "sp_Eventos_ContarPendientes");
            AgregarUsuario(comando, usuarioId);

            await conexion.OpenAsync();
            return Convert.ToInt32(await comando.ExecuteScalarAsync());
        }

        // ----- Lectura compartida -----

        private static void LlenarEvento(SqlDataReader lector, EventoTarjetaViewModel e)
        {
            e.EventoId = lector.GetInt32(lector.GetOrdinal("EventoId"));
            e.Titulo = lector.GetString(lector.GetOrdinal("Titulo"));
            e.Descripcion = lector["Descripcion"] as string;
            e.FechaEvento = lector.GetDateTime(lector.GetOrdinal("FechaEvento"));
            e.EstadoEvento = lector.GetString(lector.GetOrdinal("EstadoEvento"));
            e.MiEstado = lector.GetString(lector.GetOrdinal("MiEstado"));
            e.Categoria = lector.GetString(lector.GetOrdinal("Categoria"));
            e.SoyCreador = lector.GetBoolean(lector.GetOrdinal("SoyCreador"));
            e.LugarId = lector.GetInt32(lector.GetOrdinal("LugarId"));
            e.Lugar = lector.GetString(lector.GetOrdinal("Lugar"));
            e.Provincia = lector.GetString(lector.GetOrdinal("Provincia"));
            e.CreadorNombreUsuario = lector.GetString(lector.GetOrdinal("CreadorNombreUsuario"));
        }

        private static ParticipanteEventoViewModel LeerParticipante(SqlDataReader lector) => new()
        {
            NombreUsuario = lector.GetString(lector.GetOrdinal("NombreUsuario")),
            NombreMostrar = lector.GetString(lector.GetOrdinal("NombreMostrar")),
            AvatarUrl = lector["AvatarUrl"] as string,
            Estado = lector.GetString(lector.GetOrdinal("Estado")),
            EsCreador = lector.GetBoolean(lector.GetOrdinal("EsCreador"))
        };
    }
}