CREATE TABLE [dbo].[DisabilityTypes] (
    [Id]           INT           IDENTITY (1, 1) NOT NULL,
    [Name]         VARCHAR (255) NOT NULL,
    [ShortName]    VARCHAR (50)  CONSTRAINT [DF_DisabilityTypes_ShortName] DEFAULT ('') NOT NULL,
    [BenefitCode]  INT           CONSTRAINT [DF_DisabilityTypes_BenefitCode] DEFAULT ((0)) NOT NULL,
    [BeginDate]    DATETIME      NOT NULL,
    [EndDate]      DATETIME      NOT NULL,
    [IsDisability] BIT           CONSTRAINT [DF_DisabilityTypes_IsDisability] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_DisabilityTypes] PRIMARY KEY CLUSTERED ([Id] ASC)
);



