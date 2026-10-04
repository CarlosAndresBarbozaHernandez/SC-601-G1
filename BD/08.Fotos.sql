USE MapiticosDB;
GO

/* =====================================================================
   MAPITICOS - 07 FOTOS DE LAS AVENTURAS (AGREGAR Y BORRAR)
   1. sp_Foto_Agregar  : agrega fotos (tambien a aventuras que ya existen),
                         calcula el orden solo y permite maximo 5.
   2. sp_Lugar_Obtener : ahora devuelve el FotoId de cada foto.
   3. sp_Foto_Eliminar : borra una foto (solo el dueno de la aventura).
   Se puede ejecutar varias veces. Correr DESPUES de 06.
   ===================================================================== */


/* ---------- 1. AGREGAR FOTO ---------- */

CREATE OR ALTER PROCEDURE dbo.sp_Foto_Agregar
    @AventuraId INT,
    @UsuarioId  NVARCHAR(450),
    @Url        NVARCHAR(400),
    @Orden      INT = NULL      -- ya no se usa; se deja para no romper el codigo C#
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Aventuras
                   WHERE AventuraId = @AventuraId AND UsuarioId = @UsuarioId)
        THROW 50002, 'La aventura no existe o no es tuya.', 1;

    IF (SELECT COUNT(*) FROM dbo.FotosAventura WHERE AventuraId = @AventuraId) >= 5
        THROW 50003, 'Esta aventura ya tiene 5 fotos.', 1;

    DECLARE @Siguiente INT =
        (SELECT ISNULL(MAX(Orden), 0) + 1 FROM dbo.FotosAventura WHERE AventuraId = @AventuraId);

    INSERT INTO dbo.FotosAventura (AventuraId, Url, Orden)
    VALUES (@AventuraId, @Url, @Siguiente);
END;
GO


/* ---------- 2 y 3. VER Y BORRAR FOTOS ---------- */

CREATE OR ALTER PROCEDURE dbo.sp_Lugar_Obtener
    @LugarId   INT,
    @UsuarioId NVARCHAR(450),
    @Filtro    NVARCHAR(10) = 'todas'
AS
BEGIN
    SET NOCOUNT ON;

    SELECT v.AventuraId, v.UsuarioId
    INTO #Visibles
    FROM dbo.fn_AventurasVisibles(@UsuarioId) AS v
    WHERE v.LugarId = @LugarId;

    DECLARE @PuedeVer BIT = CASE WHEN EXISTS (SELECT 1 FROM #Visibles) THEN 1 ELSE 0 END;

    -- 1) El lugar
    SELECT
        l.LugarId,
        l.Nombre,
        pr.Nombre AS Provincia,
        l.Latitud,
        l.Longitud,
        CAST(CASE WHEN EXISTS (SELECT 1 FROM dbo.LugarFavoritos AS f
                               WHERE f.LugarId = l.LugarId AND f.UsuarioId = @UsuarioId)
             THEN 1 ELSE 0 END AS BIT) AS EsFavorito,
        (SELECT COUNT(*) FROM dbo.LugarFavoritos AS f WHERE f.LugarId = l.LugarId) AS CantidadFavoritos
    FROM dbo.Lugares AS l
    INNER JOIN dbo.Provincias AS pr ON pr.ProvinciaId = l.ProvinciaId
    WHERE l.LugarId = @LugarId AND @PuedeVer = 1;

    -- 2) Etiquetas fijas
    SELECT e.EtiquetaId, e.Nombre, e.Icono
    FROM dbo.LugarEtiquetas AS le
    INNER JOIN dbo.Etiquetas AS e ON e.EtiquetaId = le.EtiquetaId
    WHERE le.LugarId = @LugarId AND @PuedeVer = 1
    ORDER BY e.Nombre;

    -- Pestana "Amigos": solo personas que sigo con solicitud aceptada
    IF @Filtro = 'amigos'
    BEGIN
        DELETE v
        FROM #Visibles AS v
        WHERE NOT EXISTS (SELECT 1 FROM dbo.Seguimientos AS s
                          WHERE s.SeguidorId = @UsuarioId
                            AND s.SeguidoId  = v.UsuarioId
                            AND s.Estado     = 'Aceptado');
    END;

    -- 3) Las aventuras
    SELECT
        a.AventuraId,
        a.Descripcion,
        a.FechaPublicacion,
        ISNULL(p.NombreUsuario, N'usuario') AS NombreUsuario,
        ISNULL(p.NombreMostrar, ISNULL(p.NombreUsuario, N'usuario')) AS NombreMostrar,
        p.AvatarUrl,
        CAST(CASE WHEN a.UsuarioId = @UsuarioId THEN 1 ELSE 0 END AS BIT) AS EsMia,
        (SELECT COUNT(*) FROM dbo.Likes AS lk WHERE lk.AventuraId = a.AventuraId) AS CantidadLikes,
        CAST(CASE WHEN EXISTS (SELECT 1 FROM dbo.Likes AS lk
                               WHERE lk.AventuraId = a.AventuraId AND lk.UsuarioId = @UsuarioId)
             THEN 1 ELSE 0 END AS BIT) AS MeGusta,
        (SELECT COUNT(*) FROM dbo.Comentarios AS c WHERE c.AventuraId = a.AventuraId) AS CantidadComentarios
    FROM dbo.Aventuras AS a
    INNER JOIN #Visibles AS v ON v.AventuraId = a.AventuraId
    LEFT JOIN dbo.Perfiles AS p ON p.UsuarioId = a.UsuarioId
    ORDER BY a.FechaPublicacion DESC;

    -- 4) Fotos de esas aventuras
    SELECT f.FotoId, f.AventuraId, f.Url
    FROM dbo.FotosAventura AS f
    INNER JOIN #Visibles AS v ON v.AventuraId = f.AventuraId
    ORDER BY f.AventuraId, f.Orden;

    -- 5) Etiquetas personales de esas aventuras
    SELECT ae.AventuraId, e.Nombre, e.Icono
    FROM dbo.AventuraEtiquetas AS ae
    INNER JOIN #Visibles AS v ON v.AventuraId = ae.AventuraId
    INNER JOIN dbo.Etiquetas AS e ON e.EtiquetaId = ae.EtiquetaId
    ORDER BY e.Nombre;
END;
GO


-- Borra una foto. Devuelve su Url para que la app borre tambien el archivo.
-- Si la foto no existe o no es del usuario, no devuelve nada.
CREATE OR ALTER PROCEDURE dbo.sp_Foto_Eliminar
    @FotoId    INT,
    @UsuarioId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Url NVARCHAR(400);

    SELECT @Url = f.Url
    FROM dbo.FotosAventura AS f
    INNER JOIN dbo.Aventuras AS a ON a.AventuraId = f.AventuraId
    WHERE f.FotoId = @FotoId AND a.UsuarioId = @UsuarioId;

    IF @Url IS NULL RETURN;

    DELETE FROM dbo.FotosAventura WHERE FotoId = @FotoId;

    SELECT @Url AS Url;
END;
GO


/* ---------- VERIFICACION (deben salir 3 filas) ---------- */

SELECT name AS Procedimiento
FROM sys.procedures
WHERE name IN ('sp_Foto_Agregar', 'sp_Lugar_Obtener', 'sp_Foto_Eliminar')
ORDER BY name;
GO