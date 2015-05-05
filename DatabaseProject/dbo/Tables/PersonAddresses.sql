CREATE TABLE [dbo].[PersonAddresses] (
    [Id]            INT           IDENTITY (1, 1) NOT NULL,
    [PersonId]      INT           NOT NULL,
    [AddressTypeId] INT           NOT NULL,
    [OkatoText]     VARCHAR (500) NOT NULL,
    [UserText]      VARCHAR (500) NOT NULL,
    [House]         VARCHAR (50)  NOT NULL,
    [BeginDateTime] DATETIME      CONSTRAINT [DF_Addresses_InDateTime] DEFAULT (getdate()) NOT NULL,
    [EndDateTime]   DATETIME      NOT NULL,
    [Building]      VARCHAR (50)  CONSTRAINT [DF_PersonAddresses_Building] DEFAULT ('') NOT NULL,
    [Apartment]     VARCHAR (50)  CONSTRAINT [DF_PersonAddresses_Apartment] DEFAULT ('') NOT NULL,
    CONSTRAINT [PK_Addresses] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Addresses_Persons] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Persons] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_PersonAddresses_AddressTypes] FOREIGN KEY ([AddressTypeId]) REFERENCES [dbo].[AddressTypes] ([Id])
);




GO
CREATE NONCLUSTERED INDEX [IX_Addresses_4]
    ON [dbo].[PersonAddresses]([UserText] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Addresses_3]
    ON [dbo].[PersonAddresses]([PersonId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Addresses_2]
    ON [dbo].[PersonAddresses]([AddressTypeId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Addresses_1]
    ON [dbo].[PersonAddresses]([OkatoText] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Addresses]
    ON [dbo].[PersonAddresses]([BeginDateTime] DESC);

