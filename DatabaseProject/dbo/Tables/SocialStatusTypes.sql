CREATE TABLE [dbo].[SocialStatusTypes] (
    [Id]            INT           IDENTITY (1, 1) NOT NULL,
    [Name]          VARCHAR (100) NOT NULL,
    [BeginDateTime] DATETIME      NOT NULL,
    [EndDateTime]   DATETIME      NOT NULL,
    [NeedPlace]     BIT           CONSTRAINT [DF_SocialStatusTypes_NeedPlace] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_SocialStatusTypes] PRIMARY KEY CLUSTERED ([Id] ASC)
);



