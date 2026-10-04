USE MapiticosDB;
GO

/* =====================================================================
   MAPITICOS - 06 AGREGAR AVENTURA CON FOTOS

   ===================================================================== */

-- Guarda una foto de una aventura. Solo el dueno de la aventura puede.
CREATE OR ALTER PROCEDURE dbo.sp_Foto_Agregar
    @AventuraId INT,
    @UsuarioId  NVARCHAR(450),
    @Url        NVARCHAR(400),
    @Orden      INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Aventuras
                   WHERE AventuraId = @AventuraId AND UsuarioId = @UsuarioId)
        THROW 50002, 'La aventura no existe o no es tuya.', 1;

    INSERT INTO dbo.FotosAventura (AventuraId, Url, Orden)
    VALUES (@AventuraId, @Url, @Orden);
END;
GO

SELECT name AS Procedimiento FROM sys.procedures WHERE name = 'sp_Foto_Agregar';
GO