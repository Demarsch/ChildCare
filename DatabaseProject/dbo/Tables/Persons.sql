CREATE TABLE [dbo].[Persons] (
    [Id]                       INT           IDENTITY (1, 1) NOT NULL,
    [FullName]                 VARCHAR (300) NOT NULL,
    [ShortName]                VARCHAR (110) NOT NULL,
    [BirthDate]                DATETIME      NOT NULL,
    [IsMale]                   BIT           CONSTRAINT [DF_Persons_IsMale] DEFAULT ((1)) NOT NULL,
    [Snils]                    VARCHAR (50)  NOT NULL,
    [MedNumber]                VARCHAR (50)  NOT NULL,
    [PhotoId]                  INT           NULL,
    [Phones]                   VARCHAR (MAX) CONSTRAINT [DF_Persons_Phones] DEFAULT ('') NOT NULL,
    [Email]                    VARCHAR (255) CONSTRAINT [DF_Persons_Email] DEFAULT ('') NOT NULL,
    [DeleteDateTime]           DATETIME      NULL,
    [AmbNumberString]          VARCHAR (50)  NOT NULL,
    [AmbNumber]                INT           CONSTRAINT [DF_Persons_AmbNumber] DEFAULT ((0)) NOT NULL,
    [Year]                     INT           CONSTRAINT [DF_Persons_Year] DEFAULT ((0)) NOT NULL,
    [AmbNumberCreationDate]    DATETIME      NULL,
    [AmbCardFirstListHashCode] INT           CONSTRAINT [DF_Persons_AmbCardFirstList] DEFAULT ((0)) NOT NULL,
    [PersonHospListHashCode]   INT           CONSTRAINT [DF_Persons_PersonHospList] DEFAULT ((0)) NOT NULL,
    [RadiationListHashCode]    INT           CONSTRAINT [DF_Persons_RadiationList] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Persons] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Persons_Documents] FOREIGN KEY ([PhotoId]) REFERENCES [dbo].[Documents] ([Id])
);






















GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Дата и время удаления для проверки на валидность персона', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Persons', @level2type = N'COLUMN', @level2name = N'DeleteDateTime';


GO



GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Единый номер пациента', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Persons', @level2type = N'COLUMN', @level2name = N'MedNumber';

