CREATE TABLE [dbo].[Persons] (
    [Id]             INT           IDENTITY (1, 1) NOT NULL,
    [FullName]       VARCHAR (100) NOT NULL,
    [ShortName]      VARCHAR (100) NOT NULL,
    [BirthDateTime]  DATETIME      NOT NULL,
    [Snils]          VARCHAR (50)  NOT NULL,
    [MedNumber]      VARCHAR (50)  NOT NULL,
    [GenderId]       INT           NOT NULL,
    [DeleteDateTime] DATETIME      NULL,
    CONSTRAINT [PK_Persons] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Дата и время удаления для проверки на валидность персона', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Persons', @level2type = N'COLUMN', @level2name = N'DeleteDateTime';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Пол из справочника', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Persons', @level2type = N'COLUMN', @level2name = N'GenderId';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Единый номер пациента', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Persons', @level2type = N'COLUMN', @level2name = N'MedNumber';

