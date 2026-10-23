CREATE TABLE [dbo].[Books] (
    [Id]              INT             IDENTITY (17, 1) NOT NULL,
    [Name]            NVARCHAR (200)  NOT NULL,
    [Title]           NVARCHAR (200)  NOT NULL,
    [Category]        NVARCHAR (100)  NULL,
    [Publisher]       NVARCHAR (200)  NULL,
    [Author]          NVARCHAR (200)  NULL,
    [Description]     NVARCHAR (MAX)  NULL,
    [PublicationDate] DATE            NULL,
    [Height]          DECIMAL (10, 2) NULL,
    [Width]           DECIMAL (10, 2) NULL,
    [Price]           DECIMAL (18, 2) NOT NULL,
    [IsBestSeller]    BIT             CONSTRAINT [DF__Books__IsBestSel__24927208] DEFAULT ((0)) NOT NULL,
    [Genre]           NVARCHAR (100)  NULL,
    [IsInStock]       BIT             CONSTRAINT [DF__Books__IsInStock__25869641] DEFAULT ((1)) NOT NULL,
    [CreatedAt]       DATETIME2 (7)   NOT NULL,
    [UpdatedAt]       DATETIME2 (7)   NOT NULL,
    CONSTRAINT [PK_Books] PRIMARY KEY CLUSTERED ([Id] ASC)
);

