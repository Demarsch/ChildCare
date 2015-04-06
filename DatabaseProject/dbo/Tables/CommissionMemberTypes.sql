CREATE TABLE [dbo].[CommissionMemberTypes] (
    [Id]          INT           IDENTITY (1, 1) NOT NULL,
    [Name]        VARCHAR (100) NOT NULL,
    [ShortName]   VARCHAR (50)  NOT NULL,
    [IsChief]     BIT           NOT NULL,
    [IsSecretary] BIT           NOT NULL,
    [IsMember]    BIT           NOT NULL,
    CONSTRAINT [PK_CommissionMemberTypes] PRIMARY KEY CLUSTERED ([Id] ASC)
);

