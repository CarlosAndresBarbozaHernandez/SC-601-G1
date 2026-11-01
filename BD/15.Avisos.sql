USE MapiticosDB;
GO

/* =====================================================================
   MAPITICOS - 15 AVISOS
   ---------------------------------------------------------------------
   Todo lo de Avisos esta en este script:
   - Listar los avisos que me llegaron (tabla Notificaciones).
   - Marcar como leidos todos mis avisos.
   - Contar los avisos sin leer (punto del menu).
   - Dar like o comentar le deja un aviso al dueno de la aventura.
     Las acciones en mi propia aventura no dejan aviso.
     Quitar el like borra su aviso.
   Se puede ejecutar varias veces. Correr DESPUES de 05.Lugares
   (usa fn_AventurasVisibles).
   ===================================================================== */


/* =====================================================================
   1. LISTAR MIS AVISOS (los 50 mas nuevos)
      El lugar solo sale si todavia puedo ver esa aventura.
   ===================================================================== */

CREATE OR ALTER PROCEDURE dbo.sp_Notificaciones_Listar
    @UsuarioId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 50
        n.NotificacionId,
        n.Tipo,
        n.Leida,
        n.Fecha,
        ISNULL(p.NombreUsuario, N'usuario') AS NombreUsuario,
        ISNULL(p.NombreMostrar, ISNULL(p.NombreUsuario, N'usuario')) AS NombreMostrar,
        p.AvatarUrl,
        l.LugarId,
        l.Nombre AS Lugar
    FROM dbo.Notificaciones AS n
    LEFT JOIN dbo.Perfiles AS p ON p.UsuarioId = n.UsuarioOrigenId
    LEFT JOIN dbo.fn_AventurasVisibles(@UsuarioId) AS v ON v.AventuraId = n.AventuraId
    LEFT JOIN dbo.Aventuras AS a ON a.AventuraId = v.AventuraId
    LEFT JOIN dbo.Lugares AS l ON l.LugarId = a.LugarId
    WHERE n.UsuarioDestinoId = @UsuarioId
    ORDER BY n.Fecha DESC, n.NotificacionId DESC;
END;
GO


/* =====================================================================
   2. MARCAR MIS AVISOS COMO LEIDOS
   ===================================================================== */

CREATE OR ALTER PROCEDURE dbo.sp_Notificaciones_MarcarLeidas
    @UsuarioId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Notificaciones
    SET Leida = 1
    WHERE UsuarioDestinoId = @UsuarioId AND Leida = 0;
END;
GO


/* =====================================================================
   3. CONTAR MIS AVISOS SIN LEER (para el punto del menu)
   ===================================================================== */

CREATE OR ALTER PROCEDURE dbo.sp_Notificaciones_ContarSinLeer
    @UsuarioId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(*) AS CantidadSinLeer
    FROM dbo.Notificaciones
    WHERE UsuarioDestinoId = @UsuarioId AND Leida = 0;
END;
GO


/* =====================================================================
   4. LIKE (da o quita) - solo en aventuras que se pueden ver
   Si no se puede ver, no devuelve nada y la app responde "no encontrado".
   ===================================================================== */

CREATE OR ALTER PROCEDURE dbo.sp_Like_Alternar
    @UsuarioId  NVARCHAR(450),
    @AventuraId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.fn_AventurasVisibles(@UsuarioId) WHERE AventuraId = @AventuraId)
        RETURN;

    DECLARE @DuenoId NVARCHAR(450) =
        (SELECT UsuarioId FROM dbo.Aventuras WHERE AventuraId = @AventuraId);

    BEGIN TRY
        BEGIN TRANSACTION;

        IF EXISTS (SELECT 1 FROM dbo.Likes WHERE UsuarioId = @UsuarioId AND AventuraId = @AventuraId)
        BEGIN
            DELETE FROM dbo.Likes WHERE UsuarioId = @UsuarioId AND AventuraId = @AventuraId;

            -- Sin like ya no hay nada que avisar
            DELETE FROM dbo.Notificaciones
            WHERE UsuarioDestinoId = @DuenoId AND UsuarioOrigenId = @UsuarioId
              AND Tipo = 'Like' AND AventuraId = @AventuraId;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.Likes (UsuarioId, AventuraId) VALUES (@UsuarioId, @AventuraId);

            IF @DuenoId <> @UsuarioId
                INSERT INTO dbo.Notificaciones (UsuarioDestinoId, UsuarioOrigenId, Tipo, AventuraId)
                VALUES (@DuenoId, @UsuarioId, 'Like', @AventuraId);
        END;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;

    SELECT
        CAST(CASE WHEN EXISTS (SELECT 1 FROM dbo.Likes
                               WHERE UsuarioId = @UsuarioId AND AventuraId = @AventuraId)
             THEN 1 ELSE 0 END AS BIT) AS MeGusta,
        (SELECT COUNT(*) FROM dbo.Likes WHERE AventuraId = @AventuraId) AS CantidadLikes;
END;
GO


/* =====================================================================
   5. COMENTARIO - solo en aventuras que se pueden ver
   ===================================================================== */

CREATE OR ALTER PROCEDURE dbo.sp_Comentario_Agregar
    @UsuarioId NVARCHAR(450),
    @AventuraId INT,
    @Texto NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

    SET @Texto = LTRIM(RTRIM(@Texto));

    IF @Texto IS NULL OR @Texto = ''
        THROW 50001, 'El comentario no puede estar vacío.', 1;

    -- Verificar que el usuario pueda ver la aventura
    IF NOT EXISTS (
        SELECT 1
        FROM dbo.fn_AventurasVisibles(@UsuarioId)
        WHERE AventuraId = @AventuraId
    )
        THROW 50002, 'La aventura no está disponible.', 1;

    DECLARE @DuenoId NVARCHAR(450) =
        (SELECT UsuarioId FROM dbo.Aventuras WHERE AventuraId = @AventuraId);

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO dbo.Comentarios (AventuraId, UsuarioId, Texto)
        VALUES (@AventuraId, @UsuarioId, @Texto);

        IF @DuenoId <> @UsuarioId
            INSERT INTO dbo.Notificaciones (UsuarioDestinoId, UsuarioOrigenId, Tipo, AventuraId)
            VALUES (@DuenoId, @UsuarioId, 'Comentario', @AventuraId);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO


/* =====================================================================
   6. VERIFICACION (deben salir 5 procedimientos)
   ===================================================================== */

SELECT name AS Procedimiento FROM sys.procedures
WHERE name IN ('sp_Notificaciones_Listar', 'sp_Notificaciones_MarcarLeidas',
               'sp_Notificaciones_ContarSinLeer', 'sp_Like_Alternar',
               'sp_Comentario_Agregar')
ORDER BY name;
GO
