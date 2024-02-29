/****** Object:  Table [dbo].[Schools]    Script Date: 2/29/2024 6:32:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Schools](
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
	[EducationLevel] [nvarchar](100) NOT NULL,
	[SchoolType] [nvarchar](100) NOT NULL,
	[Address] [nvarchar](1000) NULL,
	[Phone] [nvarchar](1000) NULL,
	[Description] [nvarchar](1000) NULL,
	[PrincipalName] [nvarchar](1000) NULL,
	[PrincipalPhone] [nvarchar](1000) NULL,
	[PrincipalEmail] [nvarchar](1000) NULL,
	[Website] [nvarchar](1000) NULL,
	[IdPath] [nvarchar](1000) NULL,
	[LocationName] [nvarchar](1000) NULL,
	[LongPath] [nvarchar](1000) NULL,
	[ShortPath] [nvarchar](1000) NULL,
	[LocationId] [uniqueidentifier] NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_School] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Schools] ADD  DEFAULT (CONVERT([bit],(1))) FOR [IsActive]
GO
ALTER TABLE [dbo].[Schools]  WITH CHECK ADD  CONSTRAINT [FK_School_Locations_LocationId] FOREIGN KEY([LocationId])
REFERENCES [dbo].[Locations] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Schools] CHECK CONSTRAINT [FK_School_Locations_LocationId]
GO
