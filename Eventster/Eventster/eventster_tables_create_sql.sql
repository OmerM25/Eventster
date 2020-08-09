DROP TABLE [dbo].[Booking]
DROP TABLE [dbo].[User]
DROP TABLE [dbo].[Concert]
DROP TABLE [dbo].[Ticket]
DROP TABLE [dbo].[TicketType]
DROP TABLE [dbo].[Client]

CREATE TABLE [dbo].[User] (
    [UserName] CHAR (15) NOT NULL,
    [Password] CHAR (20) NOT NULL,
    PRIMARY KEY CLUSTERED ([UserName] ASC)
);


CREATE TABLE [dbo].[TicketType] (
    [Id]          INT        NOT NULL,
    [Type]        CHAR (50)  NOT NULL,
    [Description] CHAR (500) NULL,
    [Price]       INT        NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);


CREATE TABLE [dbo].[Concert] (
    [Id]         INT        NOT NULL,
    [Name]       CHAR (50)  NOT NULL,
    [Country]    CHAR (20)  NOT NULL,
    [City]       CHAR (30)  NOT NULL,
    [Address]    CHAR (50)  NOT NULL,
    [DateTime]   DATETIME   NOT NULL,
    [ArtistRank] FLOAT (53) NOT NULL,
    [XCord]      FLOAT (53) NOT NULL,
    [YCord]      FLOAT (53) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);


CREATE TABLE [dbo].[Client] (
    [Id]          CHAR (10) NOT NULL,
    [FirstName]   CHAR (10) NOT NULL,
    [LastName]    CHAR (10) NOT NULL,
    [PhoneNumber] CHAR (20) NOT NULL,
    [CreditCard]  CHAR (20) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);


CREATE TABLE [dbo].[Ticket] (
    [Id]           INT NOT NULL,
    [TicketsLeft]  INT NOT NULL,
    [ConcertId]    INT NOT NULL,
    [TicketTypeId] INT NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Ticket_ToTable] FOREIGN KEY ([ConcertId]) REFERENCES [dbo].[Concert] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Ticket_ToTable_1] FOREIGN KEY ([TicketTypeId]) REFERENCES [dbo].[TicketType] ([Id]) ON DELETE CASCADE
);


CREATE TABLE [dbo].[Booking] (
    [Id]            INT       NOT NULL,
    [ClientId]      CHAR (10) NOT NULL,
    [ConcertId]     INT       NOT NULL,
    [TicketId]      INT       NOT NULL,
    [TicketsAmount] INT       NOT NULL,
    [Id] INT NOT NULL PRIMARY KEY,
    CONSTRAINT [FK_Table_ToTable] FOREIGN KEY ([ClientId]) REFERENCES [dbo].[Client] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Table_ToTable_1] FOREIGN KEY ([ConcertId]) REFERENCES [dbo].[Concert] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Table_ToTable_2] FOREIGN KEY ([TicketId]) REFERENCES [dbo].[Ticket] ([Id])
);

