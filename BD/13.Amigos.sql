USE MapiticosDB;
GO

/* =====================================================================
   MAPITICOS - 13 BUSCAR PERSONAS Y SEGUIR
   ---------------------------------------------------------------------
   - Buscar personas por @usuario o nombre.
   - Seguir: cuenta PUBLICA  -> queda 'Aceptado' de una vez.
             cuenta PRIVADA  -> queda 'Pendiente' (solicitud).
   - Las cuentas privadas aceptan o rechazan solicitudes.
   - Ver el perfil de otra persona (con la misma regla de privacidad).
   - Se guardan avisos en Notificaciones (tipos que ya existen en la tabla).
   Se puede ejecutar varias veces. Correr DESPUES de 05.Lugares
   (usa fn_AventurasVisibles).
   ===================================================================== */


/* =====================================================================
   1. BUSCAR PERSONAS
      Devuelve mi relacion con cada una:
        EstadoSeguimiento = 'Ninguno' | 'Pendiente' | 'Aceptado'
        MeSigue           = 1 si esa persona me sigue a mi
   ===================================================================== */

CREATE OR ALTER PROCEDURE dbo.sp_Usuarios_Buscar
    @UsuarioId NVARCHAR(450),
    @Texto     NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SET @Texto = LTRIM(RTRIM(ISNULL(@Texto, '')));
    IF LEFT(@Texto, 1) = '@' SET @Texto = SUBSTRING(@Texto, 2, 100);
    IF @Texto = '' RETURN;

    -- Que % _ [ se busquen como texto normal y no como comodines
    SET @Texto = REPLACE(REPLACE(REPLACE(@Texto, '[', '[[]'), '%', '[%]'), '_', '[_]');

    SELECT TOP 20
        p.NombreUsuario,
        ISNULL(p.NombreMostrar, p.NombreUsuario) AS NombreMostrar,
        p.AvatarUrl,
        p.EsPrivada,
        ISNULL(s.Estado, 'Ninguno') AS EstadoSeguimiento,
        CAST(CASE WHEN EXISTS (SELECT 1 FROM dbo.Seguimientos AS m
                               WHERE m.SeguidorId = p.UsuarioId
                                 AND m.SeguidoId  = @UsuarioId
                                 AND m.Estado     = 'Aceptado')
             THEN 1 ELSE 0 END AS BIT) AS MeSigue
    FROM dbo.Perfiles AS p
    LEFT JOIN dbo.Seguimientos AS s
        ON s.SeguidorId = @UsuarioId AND s.SeguidoId = p.UsuarioId
    WHERE p.UsuarioId <> @UsuarioId
      AND p.NombreUsuario IS NOT NULL
      AND (p.NombreUsuario LIKE @Texto + '%'
           OR p.NombreMostrar LIKE '%' + @Texto + '%')
    ORDER BY
        CASE WHEN p.NombreUsuario LIKE @Texto + '%' THEN 0 ELSE 1 END,   -- primero los que empiezan igual
        p.NombreUsuario;
END;
GO


/* =====================================================================
   2. SEGUIR / DEJAR DE SEGUIR / CANCELAR SOLICITUD (un solo boton)
      Devuelve el estado nuevo: 'Ninguno' | 'Pendiente' | 'Aceptado'
   ===================================================================== */

CREATE OR ALTER PROCEDURE dbo.sp_Seguimiento_Alternar
    @UsuarioId     NVARCHAR(450),
    @NombreUsuario NVARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OtroId NVARCHAR(450), @EsPrivada BIT;

    SELECT @OtroId = UsuarioId, @EsPrivada = EsPrivada
    FROM dbo.Perfiles
    WHERE NombreUsuario = @NombreUsuario;

    IF @OtroId IS NULL
        THROW 52001, N'Esa persona no existe.', 1;

    IF @OtroId = @UsuarioId
        THROW 52002, N'No te podés seguir a vos misma(o).', 1;

    DECLARE @Nuevo NVARCHAR(20);

    BEGIN TRY
        BEGIN TRANSACTION;

        IF EXISTS (SELECT 1 FROM dbo.Seguimientos
                   WHERE SeguidorId = @UsuarioId AND SeguidoId = @OtroId)
        BEGIN
            -- Ya lo seguia o ya habia pedido: se quita
            DELETE FROM dbo.Seguimientos
            WHERE SeguidorId = @UsuarioId AND SeguidoId = @OtroId;

            -- Si era una solicitud sin responder, se borra su aviso
            DELETE FROM dbo.Notificaciones
            WHERE UsuarioDestinoId = @OtroId AND UsuarioOrigenId = @UsuarioId
              AND Tipo = 'SolicitudSeguimiento';

            SET @Nuevo = 'Ninguno';
        END
        ELSE
        BEGIN
            SET @Nuevo = CASE WHEN @EsPrivada = 1 THEN 'Pendiente' ELSE 'Aceptado' END;

            INSERT INTO dbo.Seguimientos (SeguidorId, SeguidoId, Estado)
            VALUES (@UsuarioId, @OtroId, @Nuevo);

            INSERT INTO dbo.Notificaciones (UsuarioDestinoId, UsuarioOrigenId, Tipo)
            VALUES (@OtroId, @UsuarioId,
                    CASE WHEN @Nuevo = 'Pendiente' THEN 'SolicitudSeguimiento' ELSE 'NuevoSeguidor' END);
        END;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;

    SELECT @Nuevo AS EstadoSeguimiento;
END;
GO


/* =====================================================================
   3. SOLICITUDES QUE ME LLEGARON (para cuentas privadas)
   ===================================================================== */

CREATE OR ALTER PROCEDURE dbo.sp_Solicitudes_Listar
    @UsuarioId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ISNULL(p.NombreUsuario, N'usuario') AS NombreUsuario,
        ISNULL(p.NombreMostrar, ISNULL(p.NombreUsuario, N'usuario')) AS NombreMostrar,
        p.AvatarUrl,
        s.FechaSolicitud
    FROM dbo.Seguimientos AS s
    LEFT JOIN dbo.Perfiles AS p ON p.UsuarioId = s.SeguidorId
    WHERE s.SeguidoId = @UsuarioId AND s.Estado = 'Pendiente'
    ORDER BY s.FechaSolicitud DESC;
END;
GO


/* =====================================================================
   4. ACEPTAR / RECHAZAR UNA SOLICITUD
      @NombreUsuario = quien me pidio seguirme
   ===================================================================== */

CREATE OR ALTER PROCEDURE dbo.sp_Solicitud_Responder
    @UsuarioId     NVARCHAR(450),
    @NombreUsuario NVARCHAR(30),
    @Acepta        BIT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @SolicitanteId NVARCHAR(450) =
        (SELECT UsuarioId FROM dbo.Perfiles WHERE NombreUsuario = @NombreUsuario);

    IF NOT EXISTS (SELECT 1 FROM dbo.Seguimientos
                   WHERE SeguidorId = @SolicitanteId AND SeguidoId = @UsuarioId
                     AND Estado = 'Pendiente')
        THROW 52003, N'Esa solicitud ya no existe.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @Acepta = 1
        BEGIN
            UPDATE dbo.Seguimientos
            SET Estado = 'Aceptado'
            WHERE SeguidorId = @SolicitanteId AND SeguidoId = @UsuarioId;

            INSERT INTO dbo.Notificaciones (UsuarioDestinoId, UsuarioOrigenId, Tipo)
            VALUES (@SolicitanteId, @UsuarioId, 'SolicitudAceptada');
        END
        ELSE
        BEGIN
            DELETE FROM dbo.Seguimientos
            WHERE SeguidorId = @SolicitanteId AND SeguidoId = @UsuarioId;
        END;

        -- El aviso de la solicitud ya se atendio
        DELETE FROM dbo.Notificaciones
        WHERE UsuarioDestinoId = @UsuarioId AND UsuarioOrigenId = @SolicitanteId
          AND Tipo = 'SolicitudSeguimiento';

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO


/* =====================================================================
   5. VER EL PERFIL DE OTRA PERSONA (tipo Instagram)
      PuedeVer = 1 si es mio, si la cuenta es publica o si la sigo (aceptado).
      Si no puede ver, solo salen los datos del encabezado (sin aventuras).
      Las aventuras salen con la misma regla de privacidad de siempre
      (fn_AventurasVisibles), asi las "Solo yo" nunca aparecen.
   ===================================================================== */

CREATE OR ALTER PROCEDURE dbo.sp_Perfil_ObtenerPublico
    @VisitanteId   NVARCHAR(450),
    @NombreUsuario NVARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DuenoId NVARCHAR(450), @EsPrivada BIT;

    SELECT @DuenoId = UsuarioId, @EsPrivada = EsPrivada
    FROM dbo.Perfiles
    WHERE NombreUsuario = @NombreUsuario;

    IF @DuenoId IS NULL RETURN;   -- no existe -> la app responde 404

    DECLARE @Estado NVARCHAR(20) = ISNULL(
        (SELECT Estado FROM dbo.Seguimientos
         WHERE SeguidorId = @VisitanteId AND SeguidoId = @DuenoId), 'Ninguno');

    DECLARE @PuedeVer BIT =
        CASE WHEN @DuenoId = @VisitanteId OR @EsPrivada = 0 OR @Estado = 'Aceptado' THEN 1 ELSE 0 END;

    -- 1) Encabezado
    SELECT
        p.NombreUsuario,
        ISNULL(p.NombreMostrar, p.NombreUsuario) AS NombreMostrar,
        p.Biografia,
        p.AvatarUrl,
        p.EsPrivada,
        (SELECT COUNT(*) FROM dbo.Seguimientos s WHERE s.SeguidoId  = @DuenoId AND s.Estado = 'Aceptado') AS CantidadSeguidores,
        (SELECT COUNT(*) FROM dbo.Seguimientos s WHERE s.SeguidorId = @DuenoId AND s.Estado = 'Aceptado') AS CantidadSiguiendo,
        (SELECT COUNT(*) FROM dbo.fn_AventurasVisibles(@VisitanteId) v WHERE v.UsuarioId = @DuenoId) AS CantidadAventurasVisibles,
        CAST(CASE WHEN @DuenoId = @VisitanteId THEN 1 ELSE 0 END AS BIT) AS EsMio,
        @Estado AS EstadoSeguimiento,
        CAST(CASE WHEN EXISTS (SELECT 1 FROM dbo.Seguimientos m
                               WHERE m.SeguidorId = @DuenoId AND m.SeguidoId = @VisitanteId
                                 AND m.Estado = 'Aceptado')
             THEN 1 ELSE 0 END AS BIT) AS MeSigue,
        @PuedeVer AS PuedeVer
    FROM dbo.Perfiles AS p
    WHERE p.UsuarioId = @DuenoId;

    -- 2) Sus aventuras que yo puedo ver
    SELECT
        a.AventuraId,
        a.Titulo,
        (SELECT TOP 1 f.Url FROM dbo.FotosAventura f
          WHERE f.AventuraId = a.AventuraId ORDER BY f.Orden) AS FotoUrl
    FROM dbo.fn_AventurasVisibles(@VisitanteId) AS v
    INNER JOIN dbo.Aventuras AS a ON a.AventuraId = v.AventuraId
    WHERE v.UsuarioId = @DuenoId AND @PuedeVer = 1
    ORDER BY a.FechaPublicacion DESC;

    -- 3) Etiquetas de esas aventuras
    SELECT ae.AventuraId, e.Nombre, e.Icono
    FROM dbo.fn_AventurasVisibles(@VisitanteId) AS v
    INNER JOIN dbo.AventuraEtiquetas AS ae ON ae.AventuraId = v.AventuraId
    INNER JOIN dbo.Etiquetas AS e ON e.EtiquetaId = ae.EtiquetaId
    WHERE v.UsuarioId = @DuenoId AND @PuedeVer = 1
    ORDER BY e.Nombre;
END;
GO


/* =====================================================================
   6. VERIFICACION (deben salir 5 procedimientos)
   ===================================================================== */

SELECT name AS Procedimiento FROM sys.procedures
WHERE name IN ('sp_Usuarios_Buscar', 'sp_Seguimiento_Alternar',
               'sp_Solicitudes_Listar', 'sp_Solicitud_Responder',
               'sp_Perfil_ObtenerPublico')
ORDER BY name;
GO
