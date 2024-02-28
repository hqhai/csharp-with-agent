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
INSERT [dbo].[Packages] ([Id], [CreatedUserId], [UpdatedUserId], [DeletedUserId], [CreatedFullName], [UpdatedFullName], [DeletedFullName], [CreatedDate], [UpdatedDate], [DeletedDate], [IsDeleted], [Code], [Name], [Price], [DescriptionStr], [MonthNumber]) VALUES (N'42d7ddb2-9f36-4f86-badc-67dc16bb722b', N'00000000-0000-0000-0000-000000000000', NULL, NULL, N'', NULL, NULL, CAST(N'2023-07-24T00:00:00.0000000' AS DateTime2), NULL, NULL, 0, N'BASIC', N'Fsel_3_Months_Beta', CAST(1000000.00 AS Decimal(18, 2)), N'[{"content":"B\u00E0i gi\u1EA3ng , b\u00E0i t\u1EADp t\u00EAn n\u1EC1n t\u1EA3ng E-learning","status":true},{"content":"Truy c\u1EADp b\u00E0i t\u1EADp h\u01B0\u1EDBng d\u1EABn, v\u00E0 b\u00E0i thi Unit","status":true},{"content":"Di\u1EC5n \u0111\u00E0n","status":true},{"content":"Gi\u1EA3ng vi\u00EAn nh\u1EADn x\u00E9t","status":false},{"content":"Truy c\u1EADp ti\u1EBFt h\u1ECDc tr\u1EF1c tuy\u1EBFn cho k\u1EF9 n\u0103ng n\u00F3i v\u1EDBi Gi\u1EA3ng vi\u00EAn","status":false}]', 3)
GO
INSERT [dbo].[Packages] ([Id], [CreatedUserId], [UpdatedUserId], [DeletedUserId], [CreatedFullName], [UpdatedFullName], [DeletedFullName], [CreatedDate], [UpdatedDate], [DeletedDate], [IsDeleted], [Code], [Name], [Price], [DescriptionStr], [MonthNumber]) VALUES (N'd13ee4ab-785a-425c-bd70-b74b61df42eb', N'00000000-0000-0000-0000-000000000000', NULL, NULL, N'', NULL, NULL, CAST(N'2023-07-24T00:00:00.0000000' AS DateTime2), NULL, NULL, 0, N'BASIC', N'Fsel_12_Months_Beta', CAST(10000000.00 AS Decimal(18, 2)), N'[{"content":"B\u00E0i gi\u1EA3ng , b\u00E0i t\u1EADp t\u00EAn n\u1EC1n t\u1EA3ng E-learning","status":true},{"content":"Truy c\u1EADp b\u00E0i t\u1EADp h\u01B0\u1EDBng d\u1EABn, v\u00E0 b\u00E0i thi Unit","status":true},{"content":"Di\u1EC5n \u0111\u00E0n","status":true},{"content":"Gi\u1EA3ng vi\u00EAn nh\u1EADn x\u00E9t","status":true},{"content":"Truy c\u1EADp ti\u1EBFt h\u1ECDc tr\u1EF1c tuy\u1EBFn cho k\u1EF9 n\u0103ng n\u00F3i v\u1EDBi Gi\u1EA3ng vi\u00EAn","status":true}]', 12)
GO
INSERT [dbo].[Packages] ([Id], [CreatedUserId], [UpdatedUserId], [DeletedUserId], [CreatedFullName], [UpdatedFullName], [DeletedFullName], [CreatedDate], [UpdatedDate], [DeletedDate], [IsDeleted], [Code], [Name], [Price], [DescriptionStr], [MonthNumber]) VALUES (N'daa6fc87-6461-49d4-b3a5-c9e4cc30bc59', N'00000000-0000-0000-0000-000000000000', NULL, NULL, N'', NULL, NULL, CAST(N'2023-07-24T00:00:00.0000000' AS DateTime2), NULL, NULL, 0, N'BASIC', N'Fsel_6_Months_Beta', CAST(3000000.00 AS Decimal(18, 2)), N'[{"content":"B\u00E0i gi\u1EA3ng , b\u00E0i t\u1EADp t\u00EAn n\u1EC1n t\u1EA3ng E-learning","status":true},{"content":"Truy c\u1EADp b\u00E0i t\u1EADp h\u01B0\u1EDBng d\u1EABn, v\u00E0 b\u00E0i thi Unit","status":true},{"content":"Di\u1EC5n \u0111\u00E0n","status":true},{"content":"Gi\u1EA3ng vi\u00EAn nh\u1EADn x\u00E9t","status":true},{"content":"Truy c\u1EADp ti\u1EBFt h\u1ECDc tr\u1EF1c tuy\u1EBFn cho k\u1EF9 n\u0103ng n\u00F3i v\u1EDBi Gi\u1EA3ng vi\u00EAn","status":false}]', 6)
GO
ALTER TABLE [dbo].[Packages] ADD  DEFAULT ((0)) FOR [MonthNumber]
GO
