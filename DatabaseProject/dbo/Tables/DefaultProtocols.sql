CREATE TABLE [dbo].[DefaultProtocols] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [RecordId]    INT            NOT NULL,
    [Description] VARCHAR (8000) CONSTRAINT [DF_DefaultProtocols_Description] DEFAULT ('') NOT NULL,
    [Conclusion]  VARCHAR (8000) CONSTRAINT [DF_DefaultProtocols_Conclusion] DEFAULT ('') NOT NULL,
    CONSTRAINT [PK_DefaultProtocols] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_DefaultProtocols_Records] FOREIGN KEY ([RecordId]) REFERENCES [dbo].[Records] ([Id])
);

