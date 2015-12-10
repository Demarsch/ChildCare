CREATE TABLE [dbo].[Rooms] (
    [Id]            INT           IDENTITY (1, 1) NOT NULL,
    [Number]        VARCHAR (10)  NOT NULL,
    [Name]          VARCHAR (100) NOT NULL,
    [BeginDateTime] DATETIME      CONSTRAINT [DF_Rooms_BeginDateTime] DEFAULT (((1)/(1))/(1900)) NOT NULL,
    [EndDateTime]   DATETIME      CONSTRAINT [DF_Rooms_BeginDateTime1] DEFAULT (((1)/(1))/(1900)) NOT NULL,
    [Options]       VARCHAR (500) CONSTRAINT [DF_Rooms_Options] DEFAULT ('') NOT NULL,
    CONSTRAINT [PK_Rooms] PRIMARY KEY CLUSTERED ([Id] ASC)
);





