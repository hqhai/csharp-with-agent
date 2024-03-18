USE [env-dev.system-service]
GO
/****** Object:  Table [dbo].[Locations]    Script Date: 2/29/2024 6:28:35 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Locations](
	[Id] [uniqueidentifier] NOT NULL,
	[CreatedUserId] [uniqueidentifier] NOT NULL,
	[UpdatedUserId] [uniqueidentifier] NULL,
	[DeletedUserId] [uniqueidentifier] NULL,
	[CreatedFullName] [nvarchar](100) NOT NULL,
	[UpdatedFullName] [nvarchar](100) NULL,
	[DeletedFullName] [nvarchar](100) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[DeletedDate] [datetime2](7) NULL,
	[IsDeleted] [bit] NOT NULL,
	[Name] [nvarchar](250) NULL,
	[Level] [int] NOT NULL,
	[Description] [nvarchar](1000) NULL,
	[Type] [nvarchar](100) NOT NULL,
	[ParentId] [uniqueidentifier] NULL,
	[UrBoxId] [int] NOT NULL,
	[IdPath] [nvarchar](250) NULL,
	[IsActive] [bit] NOT NULL,
	[LocationName] [nvarchar](250) NULL,
	[LongPath] [nvarchar](1000) NULL,
	[ShortPath] [nvarchar](1000) NULL,
 CONSTRAINT [PK_Locations] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Locations] ADD  DEFAULT (CONVERT([bit],(1))) FOR [IsActive]
GO
ALTER TABLE [dbo].[Locations]  WITH CHECK ADD  CONSTRAINT [FK_Locations_Locations_ParentId] FOREIGN KEY([ParentId])
REFERENCES [dbo].[Locations] ([Id])
GO
ALTER TABLE [dbo].[Locations] CHECK CONSTRAINT [FK_Locations_Locations_ParentId]
GO
