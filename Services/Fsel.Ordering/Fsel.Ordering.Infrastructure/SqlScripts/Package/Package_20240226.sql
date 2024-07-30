USE [env-dev.order-service]
GO
/****** Object:  Table [dbo].[Packages]    Script Date: 2/26/2024 2:19:21 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Packages](
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
	[Code] [nvarchar](100) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[DescriptionStr] [nvarchar](max) NOT NULL,
	[MonthNumber] [int] NOT NULL,
 CONSTRAINT [PK_Packages] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- Testing
UPDATE [dbo].[Packages] SET [Name] = N'Fsel_1_Month_Testing', [Code] = N'BASIC' WHERE [Id] = N'42d7ddb2-9f36-4f86-badc-67dc16bb722b'
GO
UPDATE [dbo].[Packages] SET [Name] = N'Fsel_6_Months_Testing', [Code] = N'BASIC' WHERE [Id] = N'daa6fc87-6461-49d4-b3a5-c9e4cc30bc59'
GO
UPDATE [dbo].[Packages] SET [Name] = N'Fsel_12_Months_Testing', [Code] = N'BASIC' WHERE [Id] = N'd13ee4ab-785a-425c-bd70-b74b61df42eb'
GO
-- End Testing

-- Beta
UPDATE [dbo].[Packages] SET [Name] = N'Fsel_1_Month_Beta', [Code] = N'BASIC' WHERE [Id] = N'42d7ddb2-9f36-4f86-badc-67dc16bb722b'
GO
UPDATE [dbo].[Packages] SET [Name] = N'Fsel_6_Months_Beta', [Code] = N'BASIC' WHERE [Id] = N'daa6fc87-6461-49d4-b3a5-c9e4cc30bc59'
GO
UPDATE [dbo].[Packages] SET [Name] = N'Fsel_12_Months_Beta', [Code] = N'BASIC' WHERE [Id] = N'd13ee4ab-785a-425c-bd70-b74b61df42eb'
GO
-- End Beta

-- Production
UPDATE [dbo].[Packages] SET [Name] = N'Fsel_1_Month_Production', [Code] = N'BASIC' WHERE [Id] = N'42d7ddb2-9f36-4f86-badc-67dc16bb722b'
GO
UPDATE [dbo].[Packages] SET [Name] = N'Fsel_6_Months_Production', [Code] = N'BASIC' WHERE [Id] = N'daa6fc87-6461-49d4-b3a5-c9e4cc30bc59'
GO
UPDATE [dbo].[Packages] SET [Name] = N'Fsel_12_Months_Production', [Code] = N'BASIC' WHERE [Id] = N'd13ee4ab-785a-425c-bd70-b74b61df42eb'
GO
-- End Production

ALTER TABLE [dbo].[Packages] ADD  DEFAULT ((0)) FOR [MonthNumber]
GO
