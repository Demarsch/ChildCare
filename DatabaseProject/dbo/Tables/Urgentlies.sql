CREATE TABLE [dbo].[Urgentlies] (
    [Id]            INT          IDENTITY (1, 1) NOT NULL,
    [Name]          VARCHAR (50) NOT NULL,
    [ShortName]     VARCHAR (10) CONSTRAINT [DF_Urgentlies_ShortName] DEFAULT ('') NOT NULL,
    [PrintName]     VARCHAR (50) CONSTRAINT [DF_Urgentlies_PrintName] DEFAULT ('') NOT NULL,
    [IsDefault]     BIT          CONSTRAINT [DF_Urgentlies_IsDefalut] DEFAULT ((0)) NOT NULL,
    [IsUrgently]    BIT          CONSTRAINT [DF_Urgentlies_IsUrgently] DEFAULT ((0)) NOT NULL,
    [BeginDateTime] DATETIME     NOT NULL,
    [EndDateTime]   DATETIME     NOT NULL,
    CONSTRAINT [PK_Urgentlies] PRIMARY KEY CLUSTERED ([Id] ASC)
);



