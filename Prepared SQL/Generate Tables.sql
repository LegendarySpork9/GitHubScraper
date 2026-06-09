USE [GitHub]
GO

/****** Object:  Table [dbo].[Commit]    Script Date: 29/08/2025 09:07:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Commit](
	[CommitId] [int] IDENTITY(1,1) NOT NULL,
	[RepositoryId] [int] NOT NULL,
	[AuthorId] [int] NOT NULL,
	[CommitterId] [int] NOT NULL,
	[GitHubCommitId] [varchar](50) NOT NULL,
	[Message] [varchar](max) NOT NULL,
 CONSTRAINT [PK_Commit] PRIMARY KEY CLUSTERED 
(
	[CommitId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Event]    Script Date: 29/08/2025 09:07:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Event](
	[EventId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NOT NULL,
 CONSTRAINT [PK_Event] PRIMARY KEY CLUSTERED 
(
	[EventId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Issue]    Script Date: 29/08/2025 09:07:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Issue](
	[IssueId] [int] IDENTITY(1,1) NOT NULL,
	[RepositoryId] [int] NOT NULL,
	[AssigneeId] [int] NOT NULL,
	[TypeId] [int] NOT NULL,
	[StatusId] [int] NOT NULL,
	[GitHubIssueId] [bigint] NOT NULL,
	[Title] [varchar](80) NOT NULL,
	[Number] [int] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[DateSolved] [datetime] NOT NULL,
 CONSTRAINT [PK_Issue] PRIMARY KEY CLUSTERED 
(
	[IssueId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Issue] ADD  CONSTRAINT [DF_Issue_DateSolved]  DEFAULT ('1900-01-01') FOR [DateSolved]
GO

/****** Object:  Table [dbo].[IssueAggregate]    Script Date: 29/08/2025 09:07:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[IssueAggregate](
	[IssueAggregateId] [int] IDENTITY(1,1) NOT NULL,
	[RepositoryId] [int] NOT NULL,
	[Date] [datetime] NOT NULL,
	[Created] [int] NOT NULL,
	[Solved] [int] NOT NULL,
 CONSTRAINT [PK_IssueAggregate] PRIMARY KEY CLUSTERED 
(
	[IssueAggregateId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[PullRequest]    Script Date: 29/08/2025 09:07:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PullRequest](
	[PullRequestId] [int] IDENTITY(1,1) NOT NULL,
	[RepositoryId] [int] NOT NULL,
	[AssigneeId] [int] NOT NULL,
	[TypeId] [int] NOT NULL,
	[StatusId] [int] NOT NULL,
	[GitHubPullRequestId] [bigint] NOT NULL,
	[Title] [varchar](80) NOT NULL,
	[Number] [int] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[DateSolved] [datetime] NOT NULL,
	[DateMerged] [datetime] NOT NULL,
 CONSTRAINT [PK_PullRequest] PRIMARY KEY CLUSTERED 
(
	[PullRequestId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[PullRequest] ADD  CONSTRAINT [DF_PullRequest_DateSolved]  DEFAULT ('1900-01-01') FOR [DateSolved]
GO

ALTER TABLE [dbo].[PullRequest] ADD  CONSTRAINT [DF_PullRequest_DateMerged]  DEFAULT ('1900-01-01') FOR [DateMerged]
GO

/****** Object:  Table [dbo].[Release]    Script Date: 29/08/2025 09:07:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Release](
	[ReleaseId] [int] IDENTITY(1,1) NOT NULL,
	[RepositoryId] [int] NOT NULL,
	[AuthorId] [int] NOT NULL,
	[GitHubReleaseId] [bigint] NOT NULL,
	[Name] [varchar](80) NOT NULL,
	[Draft] [bit] NOT NULL,
	[NumberOfAssets] [int] NOT NULL,
	[Body] [varchar](max) NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[DatePublished] [datetime] NOT NULL,
 CONSTRAINT [PK_Release] PRIMARY KEY CLUSTERED 
(
	[ReleaseId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Repository]    Script Date: 29/08/2025 09:07:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Repository](
	[RepositoryId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NOT NULL,
 CONSTRAINT [PK_Repository] PRIMARY KEY CLUSTERED 
(
	[RepositoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[RunHistory]    Script Date: 29/08/2025 09:07:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RunHistory](
	[RunHistoryId] [int] IDENTITY(1,1) NOT NULL,
	[RepositoryId] [int] NOT NULL,
	[RunDate] [datetime] NOT NULL,
	[Issues] [int] NOT NULL,
	[Commits] [int] NOT NULL,
	[PullRequests] [int] NOT NULL,
	[WorkflowRuns] [int] NOT NULL,
	[Releases] [int] NOT NULL,
 CONSTRAINT [PK_RunHistory] PRIMARY KEY CLUSTERED 
(
	[RunHistoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Status]    Script Date: 29/08/2025 09:07:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Status](
	[StatusId] [int] IDENTITY(1,1) NOT NULL,
	[Value] [varchar](20) NOT NULL,
 CONSTRAINT [PK_Status] PRIMARY KEY CLUSTERED 
(
	[StatusId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Type]    Script Date: 29/08/2025 09:07:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Type](
	[TypeId] [int] IDENTITY(1,1) NOT NULL,
	[Value] [varchar](20) NOT NULL,
 CONSTRAINT [PK_Type] PRIMARY KEY CLUSTERED 
(
	[TypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[User]    Script Date: 29/08/2025 09:07:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[User](
	[UserId] [int] IDENTITY(1,1) NOT NULL,
	[Username] [varchar](50) NOT NULL,
 CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Workflow]    Script Date: 29/08/2025 09:07:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Workflow](
	[WorkflowId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NOT NULL,
 CONSTRAINT [PK_Workflow] PRIMARY KEY CLUSTERED 
(
	[WorkflowId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[WorkflowRun]    Script Date: 29/08/2025 09:07:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WorkflowRun](
	[RunId] [int] IDENTITY(1,1) NOT NULL,
	[RepositoryId] [int] NOT NULL,
	[ActorId] [int] NOT NULL,
	[EventId] [int] NOT NULL,
	[WorkflowId] [int] NOT NULL,
	[StatusId] [int] NOT NULL,
	[ConclusionId] [int] NOT NULL,
	[GitHubRunId] [bigint] NOT NULL,
	[Title] [varchar](80) NOT NULL,
	[RunNumber] [int] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
 CONSTRAINT [PK_WorkflowRun] PRIMARY KEY CLUSTERED 
(
	[RunId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/* Constraints */

ALTER TABLE [dbo].[Commit]  WITH CHECK ADD  CONSTRAINT [FK_Commit_Repository] FOREIGN KEY([RepositoryId])
REFERENCES [dbo].[Repository] ([RepositoryId])
GO
ALTER TABLE [dbo].[Commit] CHECK CONSTRAINT [FK_Commit_Repository]
GO
ALTER TABLE [dbo].[Commit]  WITH CHECK ADD  CONSTRAINT [FK_Commit_User] FOREIGN KEY([AuthorId])
REFERENCES [dbo].[User] ([UserId])
GO
ALTER TABLE [dbo].[Commit] CHECK CONSTRAINT [FK_Commit_User]
GO
ALTER TABLE [dbo].[Commit]  WITH CHECK ADD  CONSTRAINT [FK_Commit_UserC] FOREIGN KEY([CommitterId])
REFERENCES [dbo].[User] ([UserId])
GO
ALTER TABLE [dbo].[Commit] CHECK CONSTRAINT [FK_Commit_UserC]
GO
ALTER TABLE [dbo].[Issue]  WITH CHECK ADD  CONSTRAINT [FK_Issue_Repository] FOREIGN KEY([RepositoryId])
REFERENCES [dbo].[Repository] ([RepositoryId])
GO
ALTER TABLE [dbo].[Issue] CHECK CONSTRAINT [FK_Issue_Repository]
GO
ALTER TABLE [dbo].[Issue]  WITH CHECK ADD  CONSTRAINT [FK_Issue_Status] FOREIGN KEY([StatusId])
REFERENCES [dbo].[Status] ([StatusId])
GO
ALTER TABLE [dbo].[Issue] CHECK CONSTRAINT [FK_Issue_Status]
GO
ALTER TABLE [dbo].[Issue]  WITH CHECK ADD  CONSTRAINT [FK_Issue_Type] FOREIGN KEY([TypeId])
REFERENCES [dbo].[Type] ([TypeId])
GO
ALTER TABLE [dbo].[Issue] CHECK CONSTRAINT [FK_Issue_Type]
GO
ALTER TABLE [dbo].[Issue]  WITH CHECK ADD  CONSTRAINT [FK_Issue_User] FOREIGN KEY([AssigneeId])
REFERENCES [dbo].[User] ([UserId])
GO
ALTER TABLE [dbo].[Issue] CHECK CONSTRAINT [FK_Issue_User]
GO
ALTER TABLE [dbo].[IssueAggregate]  WITH CHECK ADD  CONSTRAINT [FK_IssueAggregate_Repository] FOREIGN KEY([RepositoryId])
REFERENCES [dbo].[Repository] ([RepositoryId])
GO
ALTER TABLE [dbo].[IssueAggregate] CHECK CONSTRAINT [FK_IssueAggregate_Repository]
GO
ALTER TABLE [dbo].[PullRequest]  WITH CHECK ADD  CONSTRAINT [FK_PullRequest_Repository] FOREIGN KEY([RepositoryId])
REFERENCES [dbo].[Repository] ([RepositoryId])
GO
ALTER TABLE [dbo].[PullRequest] CHECK CONSTRAINT [FK_PullRequest_Repository]
GO
ALTER TABLE [dbo].[PullRequest]  WITH CHECK ADD  CONSTRAINT [FK_PullRequest_Status] FOREIGN KEY([StatusId])
REFERENCES [dbo].[Status] ([StatusId])
GO
ALTER TABLE [dbo].[PullRequest] CHECK CONSTRAINT [FK_PullRequest_Status]
GO
ALTER TABLE [dbo].[PullRequest]  WITH CHECK ADD  CONSTRAINT [FK_PullRequest_Type] FOREIGN KEY([TypeId])
REFERENCES [dbo].[Type] ([TypeId])
GO
ALTER TABLE [dbo].[PullRequest] CHECK CONSTRAINT [FK_PullRequest_Type]
GO
ALTER TABLE [dbo].[PullRequest]  WITH CHECK ADD  CONSTRAINT [FK_PullRequest_User] FOREIGN KEY([AssigneeId])
REFERENCES [dbo].[User] ([UserId])
GO
ALTER TABLE [dbo].[PullRequest] CHECK CONSTRAINT [FK_PullRequest_User]
GO
ALTER TABLE [dbo].[Release]  WITH CHECK ADD  CONSTRAINT [FK_Release_Repository] FOREIGN KEY([RepositoryId])
REFERENCES [dbo].[Repository] ([RepositoryId])
GO
ALTER TABLE [dbo].[Release] CHECK CONSTRAINT [FK_Release_Repository]
GO
ALTER TABLE [dbo].[Release]  WITH CHECK ADD  CONSTRAINT [FK_Release_User] FOREIGN KEY([AuthorId])
REFERENCES [dbo].[User] ([UserId])
GO
ALTER TABLE [dbo].[Release] CHECK CONSTRAINT [FK_Release_User]
GO
ALTER TABLE [dbo].[RunHistory]  WITH CHECK ADD  CONSTRAINT [FK_RunHistory_Repository] FOREIGN KEY([RepositoryId])
REFERENCES [dbo].[Repository] ([RepositoryId])
GO
ALTER TABLE [dbo].[RunHistory] CHECK CONSTRAINT [FK_RunHistory_Repository]
GO
ALTER TABLE [dbo].[WorkflowRun]  WITH CHECK ADD  CONSTRAINT [FK_WorkflowRun_Event] FOREIGN KEY([EventId])
REFERENCES [dbo].[Event] ([EventId])
GO
ALTER TABLE [dbo].[WorkflowRun] CHECK CONSTRAINT [FK_WorkflowRun_Event]
GO
ALTER TABLE [dbo].[WorkflowRun]  WITH CHECK ADD  CONSTRAINT [FK_WorkflowRun_Repository] FOREIGN KEY([RepositoryId])
REFERENCES [dbo].[Repository] ([RepositoryId])
GO
ALTER TABLE [dbo].[WorkflowRun] CHECK CONSTRAINT [FK_WorkflowRun_Repository]
GO
ALTER TABLE [dbo].[WorkflowRun]  WITH CHECK ADD  CONSTRAINT [FK_WorkflowRun_Status] FOREIGN KEY([StatusId])
REFERENCES [dbo].[Status] ([StatusId])
GO
ALTER TABLE [dbo].[WorkflowRun] CHECK CONSTRAINT [FK_WorkflowRun_Status]
GO
ALTER TABLE [dbo].[WorkflowRun]  WITH CHECK ADD  CONSTRAINT [FK_WorkflowRun_StatusC] FOREIGN KEY([ConclusionId])
REFERENCES [dbo].[Status] ([StatusId])
GO
ALTER TABLE [dbo].[WorkflowRun] CHECK CONSTRAINT [FK_WorkflowRun_StatusC]
GO
ALTER TABLE [dbo].[WorkflowRun]  WITH CHECK ADD  CONSTRAINT [FK_WorkflowRun_User] FOREIGN KEY([ActorId])
REFERENCES [dbo].[User] ([UserId])
GO
ALTER TABLE [dbo].[WorkflowRun] CHECK CONSTRAINT [FK_WorkflowRun_User]
GO
ALTER TABLE [dbo].[WorkflowRun]  WITH CHECK ADD  CONSTRAINT [FK_WorkflowRun_Workflow] FOREIGN KEY([WorkflowId])
REFERENCES [dbo].[Workflow] ([WorkflowId])
GO
ALTER TABLE [dbo].[WorkflowRun] CHECK CONSTRAINT [FK_WorkflowRun_Workflow]
GO
