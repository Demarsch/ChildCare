CREATE TABLE [dbo].[RecordEquipments] (
    [Id]            INT        IDENTITY (1, 1) NOT NULL,
    [RecordId]      INT        NOT NULL,
    [EquipmentId]   INT        NOT NULL,
    [BeginDateTime] DATETIME   NULL,
    [EndDateTime]   DATETIME   NULL,
    [Duration]      FLOAT (53) CONSTRAINT [DF_RecordEquipments_Duration] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_RecordEquipments] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_RecordEquipments_Eqiupments] FOREIGN KEY ([EquipmentId]) REFERENCES [dbo].[Eqiupments] ([Id]),
    CONSTRAINT [FK_RecordEquipments_Records] FOREIGN KEY ([RecordId]) REFERENCES [dbo].[Records] ([Id])
);

