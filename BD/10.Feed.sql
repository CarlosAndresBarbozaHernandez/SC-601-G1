
USE MapiticosDB;
GO

-- Lista las publicaciones que el usuario puede ver en el Feed
CREATE OR ALTER PROCEDURE dbo.sp_Feed_Listar
    @UsuarioId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        a.AventuraId,
        ISNULL(p.NombreUsuario, N'usuario') AS NombreUsuario,
        p.AvatarUrl,
        l.Nombre + ', ' + pr.Nombre AS Lugar,
        (SELECT TOP 1 f.Url
         FROM dbo.FotosAventura AS f
         WHERE f.AventuraId = a.AventuraId
         ORDER BY f.Orden) AS FotoUrl,
        ISNULL(a.Descripcion, N'') AS Descripcion,
        a.FechaPublicacion,
        (SELECT COUNT(*)
         FROM dbo.Likes AS lk
         WHERE lk.AventuraId = a.AventuraId) AS CantidadLikes,
        CAST(CASE WHEN EXISTS (
            SELECT 1
            FROM dbo.Likes AS lk
            WHERE lk.AventuraId = a.AventuraId
              AND lk.UsuarioId = @UsuarioId
        ) THEN 1 ELSE 0 END AS BIT) AS LeDiLike
    FROM dbo.fn_AventurasVisibles(@UsuarioId) AS v
    INNER JOIN dbo.Aventuras AS a ON a.AventuraId = v.AventuraId
    INNER JOIN dbo.Lugares AS l ON l.LugarId = a.LugarId
    INNER JOIN dbo.Provincias AS pr ON pr.ProvinciaId = l.ProvinciaId
    LEFT JOIN dbo.Perfiles AS p ON p.UsuarioId = a.UsuarioId
    ORDER BY a.FechaPublicacion DESC;

    -- Etiquetas de las publicaciones
    SELECT
        ae.AventuraId,
        e.EtiquetaId,
        e.Nombre,
        e.Icono
    FROM dbo.AventuraEtiquetas AS ae
    INNER JOIN dbo.Etiquetas AS e ON e.EtiquetaId = ae.EtiquetaId
    INNER JOIN dbo.fn_AventurasVisibles(@UsuarioId) AS v
        ON v.AventuraId = ae.AventuraId;
END;
GO
