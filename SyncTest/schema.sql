USE [DocumentSync]
GO

/****** Object:  Table [dbo].[dmgroupfolders]    Script Date: 02/28/2017 12:59:29 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[dmgroupfolders](
	[folder_id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](255) NULL,
	[parent_id] [int] NULL,
	[parent_path] [nvarchar](255) NULL,
	[last_updated] [datetime] NULL
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[dmgroupfolders] ADD  CONSTRAINT [DF_dmgroupfolders_last_updated]  DEFAULT (getdate()) FOR [last_updated]
GO

CREATE TABLE [dbo].[dmmasters](
	[document_id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](255) NULL,
	[folder_id] [int] NULL,
	[size] [numeric](18, 0) NULL,
	[last_updated] [datetime] NULL
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[dmmasters] ADD  CONSTRAINT [DF_dmmasters_last_updated]  DEFAULT (getdate()) FOR [last_updated]
GO

