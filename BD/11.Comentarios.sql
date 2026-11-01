
USE MapiticosDB;
GO

-- sp_Comentario_Agregar esta en 14.Avisos.sql, porque el comentario tambien deja un aviso.

-- ===== Listar comentarios =====
CREATE OR ALTER PROCEDURE dbo.sp_Comentarios_Listar
    @UsuarioId NVARCHAR(450),
    @AventuraId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Verificar que el usuario pueda ver la aventura
    IF NOT EXISTS (
        SELECT 1
        FROM dbo.fn_AventurasVisibles(@UsuarioId)
        WHERE AventuraId = @AventuraId
    )
        RETURN;

    SELECT
        c.ComentarioId,
        c.AventuraId,
        ISNULL(p.NombreUsuario, N'usuario') AS NombreUsuario,
        c.Texto,
        c.Fecha
    FROM dbo.Comentarios AS c
    LEFT JOIN dbo.Perfiles AS p ON p.UsuarioId = c.UsuarioId
    WHERE c.AventuraId = @AventuraId
    ORDER BY c.Fecha ASC, c.ComentarioId ASC;
END;
GO
