USE MapiticosDB;
GO

/* =====================================================================
   MAPITICOS - 16 AVISOS DE LIKES Y COMENTARIOS
   ---------------------------------------------------------------------
   - Dar like o comentar le deja un aviso al dueno de la aventura.
   - Las acciones en mi propia aventura no dejan aviso.
   - Quitar el like borra su aviso.
   Se puede ejecutar varias veces. Correr DESPUES de 05.Lugares y
   11.Comentarios: reemplaza sus procedimientos. Si se vuelve a correr
   alguno de esos dos, hay que correr este otra vez.
   ===================================================================== */


/* =====================================================================
   1. LIKE (da o quita) - solo en aventuras que se pueden ver
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
   2. COMENTARIO - solo en aventuras que se pueden ver
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
