CREATE TABLE [dbo].[Okatos] (
    [Id]         INT           IDENTITY (1, 1) NOT NULL,
    [CodeOKATO]  VARCHAR (11)  NOT NULL,
    [Name]       VARCHAR (250) NOT NULL,
    [FullName]   VARCHAR (500) NOT NULL,
    [CodeOKTMO]  VARCHAR (11)  CONSTRAINT [DF_OKATOs_CodeOKTMO_1] DEFAULT ('') NOT NULL,
    [RegionCode] VARCHAR (2)   CONSTRAINT [DF_OKATOs_RegionCode] DEFAULT ('') NOT NULL,
    CONSTRAINT [PK_OKATOs] PRIMARY KEY CLUSTERED ([Id] ASC)
);




GO
CREATE NONCLUSTERED INDEX [IX_Okatos]
    ON [dbo].[Okatos]([CodeOKATO] ASC);

