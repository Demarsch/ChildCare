CREATE TABLE [dbo].[FinancingSources] (
    [Id]        INT           IDENTITY (1, 1) NOT NULL,
    [Name]      VARCHAR (150) NOT NULL,
    [ShortName] VARCHAR (50)  NOT NULL,
    [IsActive]  BIT           DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_FinacingSources] PRIMARY KEY CLUSTERED ([Id] ASC)
);





