CREATE TABLE [dbo].[PaymentTypes] (
    [Id]   INT           IDENTITY (1, 1) NOT NULL,
    [Name] VARCHAR (100) NOT NULL,
    CONSTRAINT [PK_PaymentTypes] PRIMARY KEY CLUSTERED ([Id] ASC)
);

