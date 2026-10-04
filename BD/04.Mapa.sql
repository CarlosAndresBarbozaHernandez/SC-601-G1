USE MapiticosDB;
GO

/* =========================================================
   MAPITICOS - STORED PROCEDURES DEL MAPA
   Se puede ejecutar varias veces (CREATE OR ALTER).
   Requiere que ya existan las tablas de 01_Estructura.sql.
   ========================================================= */


/* =========================================================
   1. LISTAR PUNTOS DEL MAPA
   Se ven: mis aventuras + las publicas de cuentas publicas
   + las publicas de cuentas privadas que sigo (aceptado).
   Nunca se ven las archivadas.
   ========================================================= */

CREATE OR ALTER PROCEDURE dbo.sp_Mapa_Listar
    @UsuarioId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    /* Aventuras que este usuario tiene permiso de ver */
    SELECT a.AventuraId
    INTO #Visibles
    FROM dbo.Aventuras AS a
    LEFT JOIN dbo.Perfiles AS p
        ON p.UsuarioId = a.UsuarioId
    WHERE a.Archivada = 0
      AND (
            a.UsuarioId = @UsuarioId
            OR (
                a.EsPublica = 1
                AND (
                    ISNULL(p.EsPrivada, 0) = 0
                    OR EXISTS (
                        SELECT 1
                        FROM dbo.Seguimientos AS s
                        WHERE s.SeguidorId = @UsuarioId
                          AND s.SeguidoId  = a.UsuarioId
                          AND s.Estado     = 'Aceptado'
                    )
                )
            )
          );

    /* Resultado 1: informacion de los puntos */
    SELECT
        a.AventuraId,
        a.Titulo,
        a.Descripcion,
        a.Latitud,
        a.Longitud,
        ISNULL(p.NombreUsuario, N'usuario') AS NombreUsuario,   -- el @usuario, NUNCA el correo
        CAST(CASE WHEN a.UsuarioId = @UsuarioId THEN 1 ELSE 0 END AS BIT) AS EsMia,
        a.EsPublica
    FROM dbo.Aventuras AS a
    INNER JOIN #Visibles AS v
        ON v.AventuraId = a.AventuraId
    LEFT JOIN dbo.Perfiles AS p
        ON p.UsuarioId = a.UsuarioId
    ORDER BY a.AventuraId DESC;

    /* Resultado 2: etiquetas de cada punto */
    SELECT
        ae.AventuraId,
        e.EtiquetaId,
        e.Nombre,
        e.Icono
    FROM dbo.AventuraEtiquetas AS ae
    INNER JOIN #Visibles AS v
        ON v.AventuraId = ae.AventuraId
    INNER JOIN dbo.Etiquetas AS e
        ON e.EtiquetaId = ae.EtiquetaId;
END;
GO


/* =========================================================
   2. CREAR UNA AVENTURA / PUNTO DEL MAPA
   Todo en una transaccion: si fallan las etiquetas,
   no queda la aventura a medias.
   ========================================================= */

CREATE OR ALTER PROCEDURE dbo.sp_Aventura_Crear
    @UsuarioId   NVARCHAR(450),
    @ProvinciaId INT,
    @Titulo      NVARCHAR(100),
    @Descripcion NVARCHAR(1000) = NULL,
    @Latitud     DECIMAL(9,6),
    @Longitud    DECIMAL(9,6),
    @EsPublica   BIT,
    @EtiquetaIds NVARCHAR(200) = NULL      -- ej: '1,4,5'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @AventuraId INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO dbo.Aventuras
            (UsuarioId, ProvinciaId, Titulo, Descripcion, Latitud, Longitud, EsPublica)
        VALUES
            (@UsuarioId, @ProvinciaId, @Titulo, @Descripcion, @Latitud, @Longitud, @EsPublica);

        SET @AventuraId = SCOPE_IDENTITY();

        IF @EtiquetaIds IS NOT NULL AND LTRIM(RTRIM(@EtiquetaIds)) <> ''
        BEGIN
            INSERT INTO dbo.AventuraEtiquetas (AventuraId, EtiquetaId)
            SELECT DISTINCT @AventuraId, TRY_CAST(value AS INT)
            FROM STRING_SPLIT(@EtiquetaIds, ',')
            WHERE TRY_CAST(value AS INT) IS NOT NULL;
        END;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;   -- el error le llega a C#
    END CATCH;

    SELECT @AventuraId AS AventuraId;
END;
GO


/* =========================================================
   3. CATALOGOS PARA EL FORMULARIO DEL MAPA
   ========================================================= */

CREATE OR ALTER PROCEDURE dbo.sp_Provincias_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ProvinciaId, Nombre
    FROM dbo.Provincias
    ORDER BY ProvinciaId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Etiquetas_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT EtiquetaId, Nombre, Icono
    FROM dbo.Etiquetas
    ORDER BY Nombre;
END;
GO


/* =========================================================
   4. VERIFICACION (deben salir 4 filas)
   ========================================================= */

SELECT name AS Procedimiento
FROM sys.procedures
WHERE name IN ('sp_Mapa_Listar', 'sp_Aventura_Crear',
               'sp_Provincias_Listar', 'sp_Etiquetas_Listar')
ORDER BY name;
GO