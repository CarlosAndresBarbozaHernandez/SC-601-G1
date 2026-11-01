USE MapiticosDB;
GO

/* =====================================================================
   MAPITICOS - 15 AVISOS
   ---------------------------------------------------------------------
   - Listar los avisos que me llegaron (tabla Notificaciones).
   - Marcar como leidos todos mis avisos.
   Se puede ejecutar varias veces. Correr DESPUES de 05.Lugares
   (usa fn_AventurasVisibles).
   ===================================================================== */


/* =====================================================================
   1. LISTAR MIS AVISOS (los 50 mas nuevos)
      El lugar solo sale si todavia puedo ver esa aventura.
   ===================================================================== */

CREATE OR ALTER PROCEDURE dbo.sp_Notificaciones_Listar
    @UsuarioId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 50
        n.NotificacionId,
        n.Tipo,
        n.Leida,
        n.Fecha,
        ISNULL(p.NombreUsuario, N'usuario') AS NombreUsuario,
        ISNULL(p.NombreMostrar, ISNULL(p.NombreUsuario, N'usuario')) AS NombreMostrar,
        p.AvatarUrl,
        l.LugarId,
        l.Nombre AS Lugar
    FROM dbo.Notificaciones AS n
    LEFT JOIN dbo.Perfiles AS p ON p.UsuarioId = n.UsuarioOrigenId
    LEFT JOIN dbo.fn_AventurasVisibles(@UsuarioId) AS v ON v.AventuraId = n.AventuraId
    LEFT JOIN dbo.Aventuras AS a ON a.AventuraId = v.AventuraId
    LEFT JOIN dbo.Lugares AS l ON l.LugarId = a.LugarId
    WHERE n.UsuarioDestinoId = @UsuarioId
    ORDER BY n.Fecha DESC, n.NotificacionId DESC;
END;
GO


/* =====================================================================
   2. MARCAR MIS AVISOS COMO LEIDOS
   ===================================================================== */

CREATE OR ALTER PROCEDURE dbo.sp_Notificaciones_MarcarLeidas
    @UsuarioId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Notificaciones
    SET Leida = 1
    WHERE UsuarioDestinoId = @UsuarioId AND Leida = 0;
END;
GO


/* =====================================================================
   3. VERIFICACION (deben salir 2 procedimientos)
   ===================================================================== */

SELECT name AS Procedimiento FROM sys.procedures
WHERE name IN ('sp_Notificaciones_Listar', 'sp_Notificaciones_MarcarLeidas')
ORDER BY name;
GO
