USE MapiticosDB;
GO

/* =====================================================================
   MAPITICOS - 05 FAVORITOS EN EL MAPA
   El popup de cada pin ahora sabe si el lugar es favorito.
   Se puede ejecutar varias veces. Correr DESPUES de 04_Lugares.
   ===================================================================== */

CREATE OR ALTER PROCEDURE dbo.sp_Mapa_ListarLugares
    @UsuarioId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    -- 1) Los lugares
    SELECT
        l.LugarId,
        l.Nombre,
        pr.Nombre AS Provincia,
        l.Latitud,
        l.Longitud,
        COUNT(*) AS CantidadAventuras,
        CAST(MAX(CASE WHEN v.UsuarioId = @UsuarioId THEN 1 ELSE 0 END) AS BIT) AS TengoAventura,
        CAST(CASE WHEN EXISTS (SELECT 1 FROM dbo.LugarFavoritos AS f
                               WHERE f.LugarId = l.LugarId AND f.UsuarioId = @UsuarioId)
             THEN 1 ELSE 0 END AS BIT) AS EsFavorito
    FROM dbo.fn_AventurasVisibles(@UsuarioId) AS v
    INNER JOIN dbo.Lugares    AS l  ON l.LugarId = v.LugarId
    INNER JOIN dbo.Provincias AS pr ON pr.ProvinciaId = l.ProvinciaId
    GROUP BY l.LugarId, l.Nombre, pr.Nombre, l.Latitud, l.Longitud
    ORDER BY l.LugarId DESC;

    -- 2) Sus etiquetas fijas (para los filtros del mapa)
    SELECT le.LugarId, e.EtiquetaId, e.Nombre, e.Icono
    FROM dbo.LugarEtiquetas AS le
    INNER JOIN dbo.Etiquetas AS e ON e.EtiquetaId = le.EtiquetaId
    WHERE le.LugarId IN (SELECT LugarId FROM dbo.fn_AventurasVisibles(@UsuarioId));
END;
GO

-- Prueba: debe salir la columna EsFavorito al final
DECLARE @Id NVARCHAR(450) = (SELECT TOP 1 Id FROM dbo.AspNetUsers);
EXEC dbo.sp_Mapa_ListarLugares @Id;
GO