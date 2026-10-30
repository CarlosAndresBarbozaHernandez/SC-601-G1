
USE MapiticosDB;
GO

-- ===== Guardar comentario =====
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

    INSERT INTO dbo.Comentarios (AventuraId, UsuarioId, Texto)
    VALUES (@AventuraId, @UsuarioId, @Texto);
END;
GO

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
