CREATE TABLE [dbo].[Documents] (
    [Id]               INT             IDENTITY (1, 1) NOT NULL,
    [FileName]         VARCHAR (500)   NOT NULL,
    [DisplayName]      VARCHAR (500)   CONSTRAINT [DF__Documents__Displ__5E54FF49] DEFAULT ('') NOT NULL,
    [Extension]        VARCHAR (10)    CONSTRAINT [DF__Documents__Exten__5F492382] DEFAULT ('') NOT NULL,
    [FileData]         VARBINARY (MAX) NOT NULL,
    [FileSize]         BIGINT          NOT NULL,
    [UploadDate]       DATETIME        NOT NULL,
    [Description]      VARCHAR (500)   CONSTRAINT [DF__Documents__Descr__6225902D] DEFAULT ('') NOT NULL,
    [DocumentFromDate] DATETIME        NULL,
    CONSTRAINT [PK__Document__3214EC075B78929E] PRIMARY KEY CLUSTERED ([Id] ASC)
);

