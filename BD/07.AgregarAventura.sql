USE MapiticosDB;
GO

/* =====================================================================
   MAPITICOS - 06 AGREGAR AVENTURA CON FOTOS

   ===================================================================== */

-- Guarda una foto de una aventura. Solo el dueno de la aventura puede.
-- sp_Foto_Agregar esta en 08.Fotos.sql (version mas nueva).

SELECT name AS Procedimiento FROM sys.procedures WHERE name = 'sp_Foto_Agregar';
GO