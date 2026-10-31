
USE MapiticosDB;
GO

CREATE OR ALTER PROCEDURE dbo.sp_LugarFavoritos_Listar
    @UsuarioId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        l.LugarId,
        l.Nombre,
        p.Nombre AS Provincia,
        f.Estado,
        f.FechaAgregado
    FROM dbo.LugarFavoritos AS f
    INNER JOIN dbo.Lugares AS l
        ON f.LugarId = l.LugarId
    INNER JOIN dbo.Provincias AS p
        ON l.ProvinciaId = p.ProvinciaId
    WHERE f.UsuarioId = @UsuarioId
      AND EXISTS (
          SELECT 1
          FROM dbo.fn_AventurasVisibles(@UsuarioId) AS v
          WHERE v.LugarId = l.LugarId
      )
    ORDER BY f.FechaAgregado DESC;
END;
GO
