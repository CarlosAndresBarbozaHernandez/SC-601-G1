USE MapiticosDB;
GO

/* =====================================================================
   MAPITICOS - 04 LUGARES
   ---------------------------------------------------------------------
   Un LUGAR (Cerro Chirripo) puede tener muchas AVENTURAS de distintas
   personas. Las etiquetas fijas son del lugar; las de cada aventura
   son personales.
   Se puede ejecutar varias veces sin romper nada.
   Correr DESPUES de 01, 02, 03 y Mapa_StoredProcedures.
   ===================================================================== */


/* =====================================================================
   1. TABLAS NUEVAS
   ===================================================================== */

IF OBJECT_ID('dbo.Lugares', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Lugares (
        LugarId        INT IDENTITY(1,1) CONSTRAINT PK_Lugares PRIMARY KEY,
        Nombre         NVARCHAR(100) NOT NULL,
        ProvinciaId    INT NOT NULL
                       CONSTRAINT FK_Lugares_Provincias REFERENCES dbo.Provincias(ProvinciaId),
        Latitud        DECIMAL(9,6) NOT NULL,
        Longitud       DECIMAL(9,6) NOT NULL,
        CreadoPorId    NVARCHAR(450) NULL
                       CONSTRAINT FK_Lugares_Usuarios REFERENCES dbo.AspNetUsers(Id),
        FechaCreacion  DATETIME2 NOT NULL CONSTRAINT DF_Lugares_Fecha DEFAULT SYSDATETIME()
    );
END;
GO

-- Etiquetas FIJAS del lugar (las pone quien lo crea; despues solo Admin)
IF OBJECT_ID('dbo.LugarEtiquetas', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LugarEtiquetas (
        LugarId     INT NOT NULL
                    CONSTRAINT FK_LugEt_Lugares REFERENCES dbo.Lugares(LugarId) ON DELETE CASCADE,
        EtiquetaId  INT NOT NULL
                    CONSTRAINT FK_LugEt_Etiquetas REFERENCES dbo.Etiquetas(EtiquetaId),
        CONSTRAINT PK_LugarEtiquetas PRIMARY KEY (LugarId, EtiquetaId)
    );
END;
GO

-- Lugares favoritos de cada usuario ("Quiero ir" / "Ya fui")
IF OBJECT_ID('dbo.LugarFavoritos', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LugarFavoritos (
        UsuarioId      NVARCHAR(450) NOT NULL
                       CONSTRAINT FK_LugFav_Usuarios REFERENCES dbo.AspNetUsers(Id),
        LugarId        INT NOT NULL
                       CONSTRAINT FK_LugFav_Lugares REFERENCES dbo.Lugares(LugarId) ON DELETE CASCADE,
        Estado         NVARCHAR(20) NOT NULL CONSTRAINT DF_LugFav_Estado DEFAULT 'QuieroIr',
        FechaAgregado  DATETIME2 NOT NULL CONSTRAINT DF_LugFav_Fecha DEFAULT SYSDATETIME(),
        CONSTRAINT PK_LugarFavoritos PRIMARY KEY (UsuarioId, LugarId),
        CONSTRAINT CK_LugFav_Estado CHECK (Estado IN ('QuieroIr', 'Visitado'))
    );
END;
GO

-- Cada aventura pertenece a un lugar
IF COL_LENGTH('dbo.Aventuras', 'LugarId') IS NULL
BEGIN
    ALTER TABLE dbo.Aventuras ADD LugarId INT NULL
        CONSTRAINT FK_Aventuras_Lugares REFERENCES dbo.Lugares(LugarId);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_Aventuras_Lugar' AND object_id = OBJECT_ID('dbo.Aventuras'))
BEGIN
    CREATE INDEX IX_Aventuras_Lugar ON dbo.Aventuras(LugarId);
END;
GO


/* =====================================================================
   2. PASAR LAS AVENTURAS QUE YA EXISTEN A LUGARES
   Mismo nombre + misma provincia = mismo lugar.
   Si despues se corre 03_DatosPrueba otra vez, volver a correr este
   script le asigna lugar a las aventuras nuevas.
   ===================================================================== */

-- Un lugar por cada nombre + provincia que todavia no tenga lugar
INSERT INTO dbo.Lugares (Nombre, ProvinciaId, Latitud, Longitud, CreadoPorId, FechaCreacion)
SELECT a.Titulo, a.ProvinciaId, a.Latitud, a.Longitud, a.UsuarioId, a.FechaPublicacion
FROM dbo.Aventuras AS a
WHERE a.LugarId IS NULL
  AND a.AventuraId = (SELECT MIN(a2.AventuraId)
                      FROM dbo.Aventuras AS a2
                      WHERE a2.LugarId IS NULL
                        AND a2.Titulo = a.Titulo
                        AND a2.ProvinciaId = a.ProvinciaId)
  AND NOT EXISTS (SELECT 1 FROM dbo.Lugares AS l
                  WHERE l.Nombre = a.Titulo AND l.ProvinciaId = a.ProvinciaId);

-- Conectar cada aventura con su lugar
UPDATE a
SET a.LugarId = (SELECT MIN(l.LugarId)
                 FROM dbo.Lugares AS l
                 WHERE l.Nombre = a.Titulo AND l.ProvinciaId = a.ProvinciaId)
FROM dbo.Aventuras AS a
WHERE a.LugarId IS NULL;

-- Etiquetas fijas = las de la primera aventura de cada lugar
INSERT INTO dbo.LugarEtiquetas (LugarId, EtiquetaId)
SELECT DISTINCT a.LugarId, ae.EtiquetaId
FROM dbo.Aventuras AS a
INNER JOIN dbo.AventuraEtiquetas AS ae ON ae.AventuraId = a.AventuraId
WHERE a.LugarId IS NOT NULL
  AND a.AventuraId = (SELECT MIN(a2.AventuraId) FROM dbo.Aventuras AS a2 WHERE a2.LugarId = a.LugarId)
  AND NOT EXISTS (SELECT 1 FROM dbo.LugarEtiquetas AS le WHERE le.LugarId = a.LugarId);
GO


/* =====================================================================
   3. REGLA DE PRIVACIDAD EN UN SOLO LUGAR
   Aventuras que @UsuarioId puede ver: las suyas + las publicas de
   cuentas publicas + las publicas de cuentas privadas que sigue.
   Todos los SP de abajo la usan, asi la regla no se repite.
   ===================================================================== */

CREATE OR ALTER FUNCTION dbo.fn_AventurasVisibles (@UsuarioId NVARCHAR(450))
RETURNS TABLE
AS
RETURN
    SELECT a.AventuraId, a.LugarId, a.UsuarioId
    FROM dbo.Aventuras AS a
    LEFT JOIN dbo.Perfiles AS p ON p.UsuarioId = a.UsuarioId
    WHERE a.Archivada = 0
      AND (
            a.UsuarioId = @UsuarioId
            OR (
                a.EsPublica = 1
                AND (
                    ISNULL(p.EsPrivada, 0) = 0
                    OR EXISTS (SELECT 1 FROM dbo.Seguimientos AS s
                               WHERE s.SeguidorId = @UsuarioId
                                 AND s.SeguidoId  = a.UsuarioId
                                 AND s.Estado     = 'Aceptado')
                )
            )
          );
GO


/* =====================================================================
   4. CREAR AVENTURA (ahora crea el lugar o se une a uno)
   Mismos parametros de antes + @LugarId opcional, asi el codigo C#
   actual sigue funcionando sin cambios.
   ===================================================================== */

CREATE OR ALTER PROCEDURE dbo.sp_Aventura_Crear
    @UsuarioId   NVARCHAR(450),
    @ProvinciaId INT,
    @Titulo      NVARCHAR(100),
    @Descripcion NVARCHAR(1000) = NULL,
    @Latitud     DECIMAL(9,6),
    @Longitud    DECIMAL(9,6),
    @EsPublica   BIT,
    @EtiquetaIds NVARCHAR(200) = NULL,   -- ej: '1,4,5'
    @LugarId     INT = NULL              -- NULL = lugar nuevo; con valor = unirse a ese lugar
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @AventuraId INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @LugarId IS NULL
        BEGIN
            -- Lugar nuevo
            INSERT INTO dbo.Lugares (Nombre, ProvinciaId, Latitud, Longitud, CreadoPorId)
            VALUES (@Titulo, @ProvinciaId, @Latitud, @Longitud, @UsuarioId);

            SET @LugarId = SCOPE_IDENTITY();

            -- Las etiquetas de quien crea el lugar quedan como las fijas
            IF @EtiquetaIds IS NOT NULL AND LTRIM(RTRIM(@EtiquetaIds)) <> ''
            BEGIN
                INSERT INTO dbo.LugarEtiquetas (LugarId, EtiquetaId)
                SELECT DISTINCT @LugarId, TRY_CAST(value AS INT)
                FROM STRING_SPLIT(@EtiquetaIds, ',')
                WHERE TRY_CAST(value AS INT) IS NOT NULL;
            END;
        END
        ELSE
        BEGIN
            -- Unirse a un lugar: nombre, provincia y ubicacion salen del lugar
            SELECT @Titulo      = Nombre,
                   @ProvinciaId = ProvinciaId,
                   @Latitud     = Latitud,
                   @Longitud    = Longitud
            FROM dbo.Lugares
            WHERE LugarId = @LugarId;

            IF @@ROWCOUNT = 0
                THROW 50001, 'El lugar no existe.', 1;
        END;

        INSERT INTO dbo.Aventuras
            (UsuarioId, LugarId, ProvinciaId, Titulo, Descripcion, Latitud, Longitud, EsPublica)
        VALUES
            (@UsuarioId, @LugarId, @ProvinciaId, @Titulo, @Descripcion, @Latitud, @Longitud, @EsPublica);

        SET @AventuraId = SCOPE_IDENTITY();

        -- Etiquetas personales de esta aventura
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
        THROW;
    END CATCH;

    SELECT @AventuraId AS AventuraId;
END;
GO


/* =====================================================================
   5. PINES DEL MAPA = LUGARES
   Un lugar solo sale si el usuario puede ver al menos una aventura ahi.
   ===================================================================== */

-- sp_Mapa_ListarLugares esta en 06.Lugar Favoritos.sql (version mas nueva).


/* =====================================================================
   6. PANTALLA "VER LUGAR"
   @Filtro: 'todas' = todo lo visible | 'amigos' = solo gente que sigo
   Si el usuario no puede ver nada del lugar, el resultado 1 sale vacio
   y la app responde "no encontrado" (asi no se espia por la URL).
   ===================================================================== */

-- sp_Lugar_Obtener esta en 08.Fotos.sql (version mas nueva).


/* =====================================================================
   7. LIKE (da o quita) - solo en aventuras que se pueden ver
   Si no se puede ver, no devuelve nada y la app responde "no encontrado".
   ===================================================================== */

-- sp_Like_Alternar esta en 14.Avisos.sql, porque el like tambien deja un aviso.


/* =====================================================================
   8. FAVORITO (agrega o quita) - solo en lugares que se pueden ver
   ===================================================================== */

CREATE OR ALTER PROCEDURE dbo.sp_LugarFavorito_Alternar
    @UsuarioId NVARCHAR(450),
    @LugarId   INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.fn_AventurasVisibles(@UsuarioId) WHERE LugarId = @LugarId)
        RETURN;

    IF EXISTS (SELECT 1 FROM dbo.LugarFavoritos WHERE UsuarioId = @UsuarioId AND LugarId = @LugarId)
        DELETE FROM dbo.LugarFavoritos WHERE UsuarioId = @UsuarioId AND LugarId = @LugarId;
    ELSE
        INSERT INTO dbo.LugarFavoritos (UsuarioId, LugarId) VALUES (@UsuarioId, @LugarId);

    SELECT
        CAST(CASE WHEN EXISTS (SELECT 1 FROM dbo.LugarFavoritos
                               WHERE UsuarioId = @UsuarioId AND LugarId = @LugarId)
             THEN 1 ELSE 0 END AS BIT) AS EsFavorito,
        (SELECT COUNT(*) FROM dbo.LugarFavoritos WHERE LugarId = @LugarId) AS CantidadFavoritos;
END;
GO


/* =====================================================================
   9. EN QUE LUGAR ESTA UNA AVENTURA
   Para "Ver en el mapa" desde el Feed o el Perfil.
   ===================================================================== */

CREATE OR ALTER PROCEDURE dbo.sp_Aventura_ObtenerLugar
    @AventuraId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT LugarId FROM dbo.Aventuras WHERE AventuraId = @AventuraId;
END;
GO


/* =====================================================================
   10. VERIFICACION
   ===================================================================== */

-- Deben salir los lugares (los de prueba: Cerro Chirripo, Playa Conchal...)
SELECT l.LugarId, l.Nombre, COUNT(a.AventuraId) AS Aventuras
FROM dbo.Lugares AS l
LEFT JOIN dbo.Aventuras AS a ON a.LugarId = l.LugarId
GROUP BY l.LugarId, l.Nombre
ORDER BY l.LugarId;

-- Deben salir 0 aventuras sin lugar
SELECT COUNT(*) AS AventurasSinLugar FROM dbo.Aventuras WHERE LugarId IS NULL;
GO