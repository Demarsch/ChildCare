CREATE TABLE [dbo].[InsuranceCompanies] (
    [Id]        INT           IDENTITY (1, 1) NOT NULL,
    [AddressF]  VARCHAR (255) NOT NULL,
    [AddressJ]  VARCHAR (255) NOT NULL,
    [DateBegin] DATETIME      NOT NULL,
    [DateEdit]  DATETIME      NOT NULL,
    [DateEnd]   DATETIME      NOT NULL,
    [DateStart] DATETIME      NOT NULL,
    [IndexF]    VARCHAR (50)  NOT NULL,
    [IndexJ]    VARCHAR (50)  NOT NULL,
    [INN]       VARCHAR (50)  NOT NULL,
    [KPP]       VARCHAR (50)  NOT NULL,
    [DocNumber] VARCHAR (50)  NOT NULL,
    [NameSMOK]  VARCHAR (255) NOT NULL,
    [NameSMOP]  VARCHAR (255) NOT NULL,
    [OGRN]      VARCHAR (50)  NOT NULL,
    [SmoCode]   VARCHAR (50)  NOT NULL,
    [OKATO]     VARCHAR (50)  NOT NULL,
    CONSTRAINT [PK_InsuranceCompanies] PRIMARY KEY CLUSTERED ([Id] ASC)
);

