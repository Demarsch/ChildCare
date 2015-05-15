CREATE TABLE [dbo].[PersonSocialStatuses] (
    [Id]                 INT           IDENTITY (1, 1) NOT NULL,
    [PersonId]           INT           NOT NULL,
    [SocialStatusTypeId] INT           NOT NULL,
    [Office]             VARCHAR (500) CONSTRAINT [DF_PersonSocialStatuses_Office] DEFAULT ('') NOT NULL,
    [OrgId]              INT           NULL,
    [BeginDateTime]      DATETIME      NOT NULL,
    [EndDateTime]        DATETIME      NOT NULL,
    CONSTRAINT [PK_PersonSocialStatuses] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PersonSocialStatuses_Orgs] FOREIGN KEY ([OrgId]) REFERENCES [dbo].[Orgs] ([Id]),
    CONSTRAINT [FK_PersonSocialStatuses_Persons] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Persons] ([Id]),
    CONSTRAINT [FK_PersonSocialStatuses_SocialStatusTypes] FOREIGN KEY ([SocialStatusTypeId]) REFERENCES [dbo].[SocialStatusTypes] ([Id])
);

