USE MapiticosDB;
GO

/* =====================================================================
   MAPITICOS - 15 TEMAS (modo de la app)
   ---------------------------------------------------------------------
 
   ===================================================================== */

IF COL_LENGTH('dbo.Perfiles', 'Tema') IS NULL
BEGIN
    ALTER TABLE dbo.Perfiles
        ADD Tema NVARCHAR(20) NOT NULL
            CONSTRAINT DF_Perfiles_Tema DEFAULT N'bosque';
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Perfil_ObtenerTema
    @UsuarioId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Tema
    FROM dbo.Perfiles
    WHERE UsuarioId = @UsuarioId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Perfil_GuardarTema
    @UsuarioId NVARCHAR(450),
    @Tema      NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Perfiles
    SET Tema = @Tema
    WHERE UsuarioId = @UsuarioId;
END
GO