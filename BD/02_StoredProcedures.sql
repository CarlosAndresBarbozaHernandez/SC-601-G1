/* =====================================================================
   MAPITICOS - 02 STORED PROCEDURES
   ---------------------------------------------------------------------
 
   ===================================================================== */

USE MapiticosDB;
GO

/* =====================================================================
   STORED PROCEDURES
   ===================================================================== */

-- ---------- Etiquetas ----------

-- Catalogo de etiquetas (para filtros del mapa y al crear aventuras)
-- sp_Etiquetas_Listar esta en 04.Mapa.sql (version mas nueva).

-- ---------- Perfil ----------

-- Perfil completo: devuelve 3 resultados en una sola llamada
CREATE OR ALTER PROCEDURE sp_Perfil_Obtener
    @UsuarioId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    -- 1) Datos del perfil + contadores
    SELECT
        ISNULL(p.NombreUsuario, LEFT(u.UserName, CHARINDEX('@', u.UserName + '@') - 1)) AS NombreUsuario,
        ISNULL(p.NombreMostrar, LEFT(u.UserName, CHARINDEX('@', u.UserName + '@') - 1)) AS NombreMostrar,
        p.Biografia,
        p.AvatarUrl,
        ISNULL(p.EsPrivada, 0) AS EsPrivada,
        (SELECT COUNT(*) FROM Seguimientos s WHERE s.SeguidoId  = u.Id AND s.Estado = 'Aceptado') AS CantidadSeguidores,
        (SELECT COUNT(*) FROM Seguimientos s WHERE s.SeguidorId = u.Id AND s.Estado = 'Aceptado') AS CantidadSiguiendo
    FROM AspNetUsers u
    LEFT JOIN Perfiles p ON p.UsuarioId = u.Id
    WHERE u.Id = @UsuarioId;

    -- 2) Sus aventuras (no archivadas), con la primera foto
    SELECT
        a.AventuraId,
        a.Titulo,
        (SELECT TOP 1 f.Url FROM FotosAventura f
          WHERE f.AventuraId = a.AventuraId ORDER BY f.Orden) AS FotoUrl
    FROM Aventuras a
    WHERE a.UsuarioId = @UsuarioId AND a.Archivada = 0
    ORDER BY a.FechaPublicacion DESC;

    -- 3) Las etiquetas de esas aventuras
    SELECT ae.AventuraId, e.Nombre, e.Icono
    FROM AventuraEtiquetas ae
    JOIN Etiquetas e ON e.EtiquetaId = ae.EtiquetaId
    JOIN Aventuras a ON a.AventuraId = ae.AventuraId
    WHERE a.UsuarioId = @UsuarioId AND a.Archivada = 0
    ORDER BY e.Nombre;
END
GO

-- Datos actuales del perfil, para llenar el formulario de Editar perfil
CREATE OR ALTER PROCEDURE sp_Perfil_ObtenerParaEditar
    @UsuarioId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        ISNULL(p.NombreUsuario, '') AS NombreUsuario,
        ISNULL(p.NombreMostrar, LEFT(u.UserName, CHARINDEX('@', u.UserName + '@') - 1)) AS NombreMostrar,
        p.Biografia,
        p.AvatarUrl,
        ISNULL(p.EsPrivada, 0) AS EsPrivada
    FROM AspNetUsers u
    LEFT JOIN Perfiles p ON p.UsuarioId = u.Id
    WHERE u.Id = @UsuarioId;
END
GO

-- Guarda el perfil: si no existe la fila la crea, si existe la actualiza
CREATE OR ALTER PROCEDURE sp_Perfil_Guardar
    @UsuarioId      NVARCHAR(450),
    @NombreUsuario  NVARCHAR(30),
    @NombreMostrar  NVARCHAR(100),
    @Biografia      NVARCHAR(300) = NULL,
    @AvatarUrl      NVARCHAR(400) = NULL,
    @EsPrivada      BIT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Perfiles WHERE UsuarioId = @UsuarioId)
    BEGIN
        UPDATE Perfiles
        SET NombreUsuario = @NombreUsuario,
            NombreMostrar = @NombreMostrar,
            Biografia     = @Biografia,
            AvatarUrl     = @AvatarUrl,
            EsPrivada     = @EsPrivada
        WHERE UsuarioId = @UsuarioId;
    END
    ELSE
    BEGIN
        INSERT INTO Perfiles (UsuarioId, NombreUsuario, NombreMostrar, Biografia, AvatarUrl, EsPrivada)
        VALUES (@UsuarioId, @NombreUsuario, @NombreMostrar, @Biografia, @AvatarUrl, @EsPrivada);
    END
END
GO

-- ---------- Nombre de usuario ----------

-- Saber si alguien ya eligio su @usuario (NULL = todavia no)
CREATE OR ALTER PROCEDURE sp_Perfil_ObtenerNombreUsuario
    @UsuarioId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT NombreUsuario FROM Perfiles WHERE UsuarioId = @UsuarioId;
END
GO

-- Esta disponible? (no cuenta al propio usuario)
CREATE OR ALTER PROCEDURE sp_Perfil_NombreUsuarioDisponible
    @NombreUsuario NVARCHAR(30),
    @UsuarioId     NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CAST(CASE WHEN EXISTS (
                SELECT 1 FROM Perfiles
                WHERE NombreUsuario = @NombreUsuario AND UsuarioId <> @UsuarioId)
           THEN 0 ELSE 1 END AS BIT) AS Disponible;
END
GO

-- Asignar el @usuario (crea la fila del perfil si no existe)
CREATE OR ALTER PROCEDURE sp_Perfil_AsignarNombreUsuario
    @UsuarioId     NVARCHAR(450),
    @NombreUsuario NVARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Perfiles WHERE UsuarioId = @UsuarioId)
        UPDATE Perfiles SET NombreUsuario = @NombreUsuario WHERE UsuarioId = @UsuarioId;
    ELSE
        INSERT INTO Perfiles (UsuarioId, NombreMostrar, NombreUsuario)
        VALUES (@UsuarioId, @NombreUsuario, @NombreUsuario);
END
GO

PRINT '02_StoredProcedures listo.';
GO
