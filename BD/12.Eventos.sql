USE MapiticosDB;
GO

/* =====================================================================
   MAPITICOS - 12 EVENTOS (planear salidas a un lugar favorito)
   ---------------------------------------------------------------------
   - Un EVENTO es una salida a un LUGAR en una fecha, creada por alguien.
   - El creador invita amigos; cada invitado acepta o rechaza.
   - Los que aceptaron (y el creador) tienen un chat del evento.
   - El creador lo marca como HECHO (y el lugar pasa a "Ya fui").

   Como lo ve cada usuario (columna Categoria):
     Pendiente  = me invitaron y todavia no respondo
     Programado = voy y todavia no se hizo
     Hecho      = ya se hizo

   Se puede ejecutar varias veces. Correr DESPUES de 05.Lugares.
   ===================================================================== */


/* =====================================================================
   1. TABLAS
   ===================================================================== */

IF OBJECT_ID('dbo.Eventos', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Eventos (
        EventoId       INT IDENTITY(1,1) CONSTRAINT PK_Eventos PRIMARY KEY,
        CreadorId      NVARCHAR(450) NOT NULL
                       CONSTRAINT FK_Eventos_Usuarios REFERENCES dbo.AspNetUsers(Id),
        LugarId        INT NOT NULL
                       CONSTRAINT FK_Eventos_Lugares REFERENCES dbo.Lugares(LugarId) ON DELETE CASCADE,
        Titulo         NVARCHAR(100) NOT NULL,
        Descripcion    NVARCHAR(500) NULL,
        FechaEvento    DATETIME2 NOT NULL,
        Estado         NVARCHAR(20) NOT NULL CONSTRAINT DF_Eventos_Estado DEFAULT 'Programado',
        FechaCreacion  DATETIME2 NOT NULL CONSTRAINT DF_Eventos_Fecha DEFAULT SYSDATETIME(),
        FechaHecho     DATETIME2 NULL,
        CONSTRAINT CK_Eventos_Estado CHECK (Estado IN ('Programado', 'Hecho'))
    );
END;
GO

-- El creador tambien se guarda aqui (como 'Aceptado'): asi el chat y las
-- listas tratan a todos igual.
IF OBJECT_ID('dbo.EventoParticipantes', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EventoParticipantes (
        EventoId         INT NOT NULL
                         CONSTRAINT FK_EvPar_Eventos REFERENCES dbo.Eventos(EventoId) ON DELETE CASCADE,
        UsuarioId        NVARCHAR(450) NOT NULL
                         CONSTRAINT FK_EvPar_Usuarios REFERENCES dbo.AspNetUsers(Id),
        Estado           NVARCHAR(20) NOT NULL CONSTRAINT DF_EvPar_Estado DEFAULT 'Pendiente',
        FechaInvitacion  DATETIME2 NOT NULL CONSTRAINT DF_EvPar_Fecha DEFAULT SYSDATETIME(),
        FechaRespuesta   DATETIME2 NULL,
        CONSTRAINT PK_EventoParticipantes PRIMARY KEY (EventoId, UsuarioId),
        CONSTRAINT CK_EvPar_Estado CHECK (Estado IN ('Pendiente', 'Aceptado', 'Rechazado'))
    );
END;
GO

IF OBJECT_ID('dbo.EventoMensajes', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EventoMensajes (
        MensajeId  INT IDENTITY(1,1) CONSTRAINT PK_EventoMensajes PRIMARY KEY,
        EventoId   INT NOT NULL
                   CONSTRAINT FK_EvMsj_Eventos REFERENCES dbo.Eventos(EventoId) ON DELETE CASCADE,
        UsuarioId  NVARCHAR(450) NOT NULL
                   CONSTRAINT FK_EvMsj_Usuarios REFERENCES dbo.AspNetUsers(Id),
        Texto      NVARCHAR(500) NOT NULL,
        Fecha      DATETIME2 NOT NULL CONSTRAINT DF_EvMsj_Fecha DEFAULT SYSDATETIME()
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_EvPar_Usuario' AND object_id = OBJECT_ID('dbo.EventoParticipantes'))
    CREATE INDEX IX_EvPar_Usuario ON dbo.EventoParticipantes(UsuarioId, Estado);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_EvMsj_Evento' AND object_id = OBJECT_ID('dbo.EventoMensajes'))
    CREATE INDEX IX_EvMsj_Evento ON dbo.EventoMensajes(EventoId, MensajeId);
GO


/* =====================================================================
   2. AMIGOS = personas con las que tengo seguimiento aceptado
      (yo las sigo o ellas me siguen). Solo con @usuario elegido.
   ===================================================================== */

CREATE OR ALTER FUNCTION dbo.fn_Amigos (@UsuarioId NVARCHAR(450))
RETURNS TABLE
AS
RETURN
    SELECT DISTINCT x.AmigoId
    FROM (
        SELECT s.SeguidoId AS AmigoId FROM dbo.Seguimientos AS s
        WHERE s.SeguidorId = @UsuarioId AND s.Estado = 'Aceptado'
        UNION
        SELECT s.SeguidorId FROM dbo.Seguimientos AS s
        WHERE s.SeguidoId = @UsuarioId AND s.Estado = 'Aceptado'
    ) AS x
    INNER JOIN dbo.Perfiles AS p ON p.UsuarioId = x.AmigoId
    WHERE p.NombreUsuario IS NOT NULL;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Amigos_Listar
    @UsuarioId NVARCHAR(450),
    @EventoId  INT = NULL      -- si viene, marca quien ya esta en ese evento
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.NombreUsuario,
        ISNULL(p.NombreMostrar, p.NombreUsuario) AS NombreMostrar,
        p.AvatarUrl,
        CAST(CASE WHEN EXISTS (SELECT 1 FROM dbo.EventoParticipantes AS ep
                               WHERE ep.EventoId = @EventoId
                                 AND ep.UsuarioId = a.AmigoId
                                 AND ep.Estado <> 'Rechazado')
             THEN 1 ELSE 0 END AS BIT) AS YaInvitado
    FROM dbo.fn_Amigos(@UsuarioId) AS a
    INNER JOIN dbo.Perfiles AS p ON p.UsuarioId = a.AmigoId
    ORDER BY ISNULL(p.NombreMostrar, p.NombreUsuario);
END;
GO


/* =====================================================================
   3. INVITAR (uso interno de Crear e Invitar)
      @Invitados = '@usuarios' separados por coma: 'ana,luis_cr'
      Solo entran amigos reales; si alguien habia rechazado, se le
      vuelve a invitar.
   ===================================================================== */

CREATE OR ALTER PROCEDURE dbo.sp_Evento_AgregarInvitados
    @EventoId  INT,
    @CreadorId NVARCHAR(450),
    @Invitados NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT p.UsuarioId
    INTO #Nuevos
    FROM STRING_SPLIT(ISNULL(@Invitados, ''), ',') AS v
    INNER JOIN dbo.Perfiles AS p ON p.NombreUsuario = LTRIM(RTRIM(v.value))
    INNER JOIN dbo.fn_Amigos(@CreadorId) AS a ON a.AmigoId = p.UsuarioId;

    -- Re-invitar a quien habia rechazado
    UPDATE ep
    SET ep.Estado = 'Pendiente', ep.FechaInvitacion = SYSDATETIME(), ep.FechaRespuesta = NULL
    FROM dbo.EventoParticipantes AS ep
    INNER JOIN #Nuevos AS n ON n.UsuarioId = ep.UsuarioId
    WHERE ep.EventoId = @EventoId AND ep.Estado = 'Rechazado';

    -- Invitar a los nuevos
    INSERT INTO dbo.EventoParticipantes (EventoId, UsuarioId, Estado)
    SELECT @EventoId, n.UsuarioId, 'Pendiente'
    FROM #Nuevos AS n
    WHERE NOT EXISTS (SELECT 1 FROM dbo.EventoParticipantes AS ep
                      WHERE ep.EventoId = @EventoId AND ep.UsuarioId = n.UsuarioId);
END;
GO


/* =====================================================================
   4. CREAR EVENTO
   ===================================================================== */

CREATE OR ALTER PROCEDURE dbo.sp_Evento_Crear
    @CreadorId   NVARCHAR(450),
    @LugarId     INT,
    @Titulo      NVARCHAR(100),
    @Descripcion NVARCHAR(500) = NULL,
    @FechaEvento DATETIME2,
    @Invitados   NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Solo lugares que el creador puede ver (misma regla de privacidad de siempre)
    IF NOT EXISTS (SELECT 1 FROM dbo.fn_AventurasVisibles(@CreadorId) WHERE LugarId = @LugarId)
        THROW 51001, N'El lugar no existe o no está disponible.', 1;

    DECLARE @EventoId INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO dbo.Eventos (CreadorId, LugarId, Titulo, Descripcion, FechaEvento)
        VALUES (@CreadorId, @LugarId, LTRIM(RTRIM(@Titulo)), NULLIF(LTRIM(RTRIM(@Descripcion)), ''), @FechaEvento);

        SET @EventoId = SCOPE_IDENTITY();

        -- El creador va de una vez como aceptado
        INSERT INTO dbo.EventoParticipantes (EventoId, UsuarioId, Estado, FechaRespuesta)
        VALUES (@EventoId, @CreadorId, 'Aceptado', SYSDATETIME());

        EXEC dbo.sp_Evento_AgregarInvitados @EventoId, @CreadorId, @Invitados;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;

    SELECT @EventoId AS EventoId;
END;
GO


/* =====================================================================
   5. INVITAR MAS AMIGOS (solo el creador, solo si no esta hecho)
   ===================================================================== */

CREATE OR ALTER PROCEDURE dbo.sp_Evento_Invitar
    @EventoId  INT,
    @UsuarioId NVARCHAR(450),
    @Invitados NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Eventos
                   WHERE EventoId = @EventoId AND CreadorId = @UsuarioId)
        THROW 51002, N'Solo quien creó el evento puede invitar.', 1;

    IF EXISTS (SELECT 1 FROM dbo.Eventos WHERE EventoId = @EventoId AND Estado = 'Hecho')
        THROW 51003, N'Este evento ya se marcó como hecho.', 1;

    EXEC dbo.sp_Evento_AgregarInvitados @EventoId, @UsuarioId, @Invitados;
END;
GO


/* =====================================================================
   6. LISTAR MIS EVENTOS (pantalla Eventos)
      Resultado 1: eventos   |   Resultado 2: participantes de cada uno
   ===================================================================== */

CREATE OR ALTER PROCEDURE dbo.sp_Eventos_Listar
    @UsuarioId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT e.EventoId, yo.Estado AS MiEstado
    INTO #Mios
    FROM dbo.Eventos AS e
    INNER JOIN dbo.EventoParticipantes AS yo
        ON yo.EventoId = e.EventoId AND yo.UsuarioId = @UsuarioId
    WHERE yo.Estado <> 'Rechazado';

    SELECT
        e.EventoId,
        e.Titulo,
        e.Descripcion,
        e.FechaEvento,
        e.Estado AS EstadoEvento,
        m.MiEstado,
        CASE WHEN m.MiEstado = 'Pendiente' THEN 'Pendiente'
             WHEN e.Estado   = 'Hecho'     THEN 'Hecho'
             ELSE 'Programado' END AS Categoria,
        CAST(CASE WHEN e.CreadorId = @UsuarioId THEN 1 ELSE 0 END AS BIT) AS SoyCreador,
        l.LugarId,
        l.Nombre AS Lugar,
        pr.Nombre AS Provincia,
        ISNULL(pc.NombreUsuario, N'usuario') AS CreadorNombreUsuario
    FROM #Mios AS m
    INNER JOIN dbo.Eventos    AS e  ON e.EventoId = m.EventoId
    INNER JOIN dbo.Lugares    AS l  ON l.LugarId = e.LugarId
    INNER JOIN dbo.Provincias AS pr ON pr.ProvinciaId = l.ProvinciaId
    LEFT  JOIN dbo.Perfiles   AS pc ON pc.UsuarioId = e.CreadorId
    ORDER BY
        CASE WHEN m.MiEstado = 'Pendiente' THEN 0 WHEN e.Estado = 'Programado' THEN 1 ELSE 2 END,
        CASE WHEN e.Estado = 'Programado' THEN e.FechaEvento END ASC,   -- proximos primero
        e.FechaEvento DESC;                                             -- hechos: recientes primero

    SELECT
        ep.EventoId,
        ISNULL(p.NombreUsuario, N'usuario') AS NombreUsuario,
        ISNULL(p.NombreMostrar, ISNULL(p.NombreUsuario, N'usuario')) AS NombreMostrar,
        p.AvatarUrl,
        ep.Estado,
        CAST(CASE WHEN ep.UsuarioId = e.CreadorId THEN 1 ELSE 0 END AS BIT) AS EsCreador
    FROM dbo.EventoParticipantes AS ep
    INNER JOIN #Mios       AS m ON m.EventoId = ep.EventoId
    INNER JOIN dbo.Eventos AS e ON e.EventoId = ep.EventoId
    LEFT  JOIN dbo.Perfiles AS p ON p.UsuarioId = ep.UsuarioId
    WHERE ep.Estado <> 'Rechazado'
    ORDER BY ep.EventoId, EsCreador DESC, ep.FechaInvitacion;
END;
GO


/* =====================================================================
   7. VER UN EVENTO
      Si no soy participante (o rechace), no devuelve nada -> "no encontrado".
   ===================================================================== */

CREATE OR ALTER PROCEDURE dbo.sp_Evento_Obtener
    @EventoId  INT,
    @UsuarioId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @MiEstado NVARCHAR(20) =
        (SELECT Estado FROM dbo.EventoParticipantes
         WHERE EventoId = @EventoId AND UsuarioId = @UsuarioId);

    IF @MiEstado IS NULL OR @MiEstado = 'Rechazado'
        RETURN;

    -- 1) El evento
    SELECT
        e.EventoId,
        e.Titulo,
        e.Descripcion,
        e.FechaEvento,
        e.Estado AS EstadoEvento,
        e.FechaHecho,
        @MiEstado AS MiEstado,
        CASE WHEN @MiEstado = 'Pendiente' THEN 'Pendiente'
             WHEN e.Estado  = 'Hecho'     THEN 'Hecho'
             ELSE 'Programado' END AS Categoria,
        CAST(CASE WHEN e.CreadorId = @UsuarioId THEN 1 ELSE 0 END AS BIT) AS SoyCreador,
        l.LugarId,
        l.Nombre AS Lugar,
        pr.Nombre AS Provincia,
        ISNULL(pc.NombreUsuario, N'usuario') AS CreadorNombreUsuario
    FROM dbo.Eventos AS e
    INNER JOIN dbo.Lugares    AS l  ON l.LugarId = e.LugarId
    INNER JOIN dbo.Provincias AS pr ON pr.ProvinciaId = l.ProvinciaId
    LEFT  JOIN dbo.Perfiles   AS pc ON pc.UsuarioId = e.CreadorId
    WHERE e.EventoId = @EventoId;

    -- 2) Participantes (tambien quien rechazo, para que el creador lo sepa)
    SELECT
        ep.EventoId,
        ISNULL(p.NombreUsuario, N'usuario') AS NombreUsuario,
        ISNULL(p.NombreMostrar, ISNULL(p.NombreUsuario, N'usuario')) AS NombreMostrar,
        p.AvatarUrl,
        ep.Estado,
        CAST(CASE WHEN ep.UsuarioId = e.CreadorId THEN 1 ELSE 0 END AS BIT) AS EsCreador
    FROM dbo.EventoParticipantes AS ep
    INNER JOIN dbo.Eventos  AS e ON e.EventoId = ep.EventoId
    LEFT  JOIN dbo.Perfiles AS p ON p.UsuarioId = ep.UsuarioId
    WHERE ep.EventoId = @EventoId
    ORDER BY EsCreador DESC,
             CASE ep.Estado WHEN 'Aceptado' THEN 0 WHEN 'Pendiente' THEN 1 ELSE 2 END,
             ep.FechaInvitacion;
END;
GO


/* =====================================================================
   8. RESPONDER INVITACION (aceptar / rechazar)
   ===================================================================== */

CREATE OR ALTER PROCEDURE dbo.sp_Evento_Responder
    @EventoId  INT,
    @UsuarioId NVARCHAR(450),
    @Acepta    BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.EventoParticipantes
    SET Estado = CASE WHEN @Acepta = 1 THEN 'Aceptado' ELSE 'Rechazado' END,
        FechaRespuesta = SYSDATETIME()
    WHERE EventoId = @EventoId AND UsuarioId = @UsuarioId AND Estado = 'Pendiente';

    IF @@ROWCOUNT = 0
        THROW 51004, N'No tenés una invitación pendiente para este evento.', 1;
END;
GO


/* =====================================================================
   9. SALIR DE UN EVENTO (un invitado que habia aceptado y ya no va)
   ===================================================================== */

CREATE OR ALTER PROCEDURE dbo.sp_Evento_Salir
    @EventoId  INT,
    @UsuarioId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.Eventos WHERE EventoId = @EventoId AND CreadorId = @UsuarioId)
        THROW 51005, N'Sos quien organiza: si ya no se hace, cancelá el evento.', 1;

    UPDATE dbo.EventoParticipantes
    SET Estado = 'Rechazado', FechaRespuesta = SYSDATETIME()
    WHERE EventoId = @EventoId AND UsuarioId = @UsuarioId AND Estado = 'Aceptado';
END;
GO


/* =====================================================================
   10. MARCAR COMO HECHO / VOLVER A PROGRAMADO (solo el creador)
       Al marcarlo hecho, a quienes fueron y tienen el lugar en
       favoritos se les cambia a "Ya fui" (Visitado).
   ===================================================================== */

CREATE OR ALTER PROCEDURE dbo.sp_Evento_CambiarEstado
    @EventoId  INT,
    @UsuarioId NVARCHAR(450),
    @Hecho     BIT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Eventos
                   WHERE EventoId = @EventoId AND CreadorId = @UsuarioId)
        THROW 51006, N'Solo quien creó el evento puede cambiar su estado.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.Eventos
        SET Estado     = CASE WHEN @Hecho = 1 THEN 'Hecho' ELSE 'Programado' END,
            FechaHecho = CASE WHEN @Hecho = 1 THEN SYSDATETIME() ELSE NULL END
        WHERE EventoId = @EventoId;

        IF @Hecho = 1
        BEGIN
            UPDATE f
            SET f.Estado = 'Visitado'
            FROM dbo.LugarFavoritos AS f
            INNER JOIN dbo.Eventos AS e ON e.LugarId = f.LugarId
            INNER JOIN dbo.EventoParticipantes AS ep
                ON ep.EventoId = e.EventoId AND ep.UsuarioId = f.UsuarioId
            WHERE e.EventoId = @EventoId AND ep.Estado = 'Aceptado';
        END;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO


/* =====================================================================
   11. CANCELAR (BORRAR) EVENTO (solo el creador)
       Participantes y mensajes se borran solos (ON DELETE CASCADE).
   ===================================================================== */

CREATE OR ALTER PROCEDURE dbo.sp_Evento_Eliminar
    @EventoId  INT,
    @UsuarioId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.Eventos WHERE EventoId = @EventoId AND CreadorId = @UsuarioId;

    IF @@ROWCOUNT = 0
        THROW 51007, N'Solo quien creó el evento puede cancelarlo.', 1;
END;
GO


/* =====================================================================
   12. CHAT DEL EVENTO (solo participantes que aceptaron)
       @DesdeId: la app pide solo los mensajes nuevos (MensajeId > @DesdeId)
   ===================================================================== */

CREATE OR ALTER PROCEDURE dbo.sp_EventoMensajes_Listar
    @EventoId  INT,
    @UsuarioId NVARCHAR(450),
    @DesdeId   INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.EventoParticipantes
                   WHERE EventoId = @EventoId AND UsuarioId = @UsuarioId AND Estado = 'Aceptado')
        RETURN;

    SELECT
        m.MensajeId,
        ISNULL(p.NombreUsuario, N'usuario') AS NombreUsuario,
        ISNULL(p.NombreMostrar, ISNULL(p.NombreUsuario, N'usuario')) AS NombreMostrar,
        p.AvatarUrl,
        m.Texto,
        m.Fecha,
        CAST(CASE WHEN m.UsuarioId = @UsuarioId THEN 1 ELSE 0 END AS BIT) AS EsMio
    FROM dbo.EventoMensajes AS m
    LEFT JOIN dbo.Perfiles AS p ON p.UsuarioId = m.UsuarioId
    WHERE m.EventoId = @EventoId AND m.MensajeId > @DesdeId
    ORDER BY m.MensajeId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_EventoMensaje_Agregar
    @EventoId  INT,
    @UsuarioId NVARCHAR(450),
    @Texto     NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

    SET @Texto = LTRIM(RTRIM(@Texto));

    IF @Texto IS NULL OR @Texto = ''
        THROW 51008, N'El mensaje no puede estar vacío.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.EventoParticipantes
                   WHERE EventoId = @EventoId AND UsuarioId = @UsuarioId AND Estado = 'Aceptado')
        THROW 51009, N'Tenés que aceptar la invitación para escribir en el chat.', 1;

    INSERT INTO dbo.EventoMensajes (EventoId, UsuarioId, Texto)
    VALUES (@EventoId, @UsuarioId, @Texto);
END;
GO


/* =====================================================================
   13. CUANTAS INVITACIONES TENGO SIN RESPONDER (para el globito)
   ===================================================================== */

CREATE OR ALTER PROCEDURE dbo.sp_Eventos_ContarPendientes
    @UsuarioId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) AS Pendientes
    FROM dbo.EventoParticipantes
    WHERE UsuarioId = @UsuarioId AND Estado = 'Pendiente';
END;
GO


/* =====================================================================
   14. VERIFICACION (deben salir 3 tablas y 13 procedimientos)
   ===================================================================== */

SELECT name AS Tabla FROM sys.tables
WHERE name IN ('Eventos', 'EventoParticipantes', 'EventoMensajes')
ORDER BY name;

SELECT name AS Procedimiento FROM sys.procedures
WHERE name IN ('sp_Amigos_Listar', 'sp_Evento_AgregarInvitados', 'sp_Evento_Crear',
               'sp_Evento_Invitar', 'sp_Eventos_Listar', 'sp_Evento_Obtener',
               'sp_Evento_Responder', 'sp_Evento_Salir', 'sp_Evento_CambiarEstado',
               'sp_Evento_Eliminar', 'sp_EventoMensajes_Listar', 'sp_EventoMensaje_Agregar',
               'sp_Eventos_ContarPendientes')
ORDER BY name;
GO