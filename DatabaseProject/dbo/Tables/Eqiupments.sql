CREATE TABLE [dbo].[Eqiupments] (
    [Id]                INT           IDENTITY (1, 1) NOT NULL,
    [EquipmentTypeId]   INT           NOT NULL,
    [Name]              VARCHAR (255) NOT NULL,
    [RoomId]            INT           NULL,
    [BranchId]          INT           NULL,
    [CatalogNumber]     VARCHAR (100) CONSTRAINT [DF_Eqiupments_CatalogNumber] DEFAULT ('') NOT NULL,
    [NameToEconomics]   VARCHAR (555) CONSTRAINT [DF_Eqiupments_NameToEconomics] DEFAULT ('') NOT NULL,
    [ProductionDate]    DATETIME      CONSTRAINT [DF_Eqiupments_ProductionDate] DEFAULT (((1)/(1))/(1900)) NOT NULL,
    [StartUseDate]      DATETIME      CONSTRAINT [DF_Eqiupments_StartUseDate] DEFAULT (((1)/(1))/(1900)) NOT NULL,
    [EndUseDate]        DATETIME      CONSTRAINT [DF_Eqiupments_EndUseDate] DEFAULT (((31)/(12))/(9999)) NOT NULL,
    [SertificateNumber] VARCHAR (50)  CONSTRAINT [DF_Eqiupments_SertificateNumber] DEFAULT ('') NOT NULL,
    [InventoryNumber]   VARCHAR (50)  CONSTRAINT [DF_Eqiupments_InventoryNumber] DEFAULT ('') NOT NULL,
    [PhotoPath]         VARCHAR (500) CONSTRAINT [DF_Eqiupments_PhotoPath] DEFAULT ('') NOT NULL,
    [IPAddress]         VARCHAR (16)  CONSTRAINT [DF_Eqiupments_IPAddress] DEFAULT ('') NOT NULL,
    CONSTRAINT [PK_Eqiupments] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Eqiupments_Branches] FOREIGN KEY ([BranchId]) REFERENCES [dbo].[Branches] ([Id]),
    CONSTRAINT [FK_Eqiupments_EqiupmentTypes] FOREIGN KEY ([EquipmentTypeId]) REFERENCES [dbo].[EqiupmentTypes] ([Id]),
    CONSTRAINT [FK_Eqiupments_Rooms] FOREIGN KEY ([RoomId]) REFERENCES [dbo].[Rooms] ([Id])
);

