USE [GitHub]
GO

/****** Object:  StoredProcedure [dbo].[StoreIssue]    Script Date: 05/09/2025 10:38:42 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Toby Hunter
-- Create date: 05/09/2025
-- Description:	Procedure for logging issues collected from GitHub.
-- =============================================
CREATE PROCEDURE [dbo].[StoreIssue] 
	@repository varchar(50),
	@issueId bigint,
	@number int,
	@title varchar(80),
	@assignee varchar(50),
	@type varchar(20),
	@status varchar(20),
	@dateCreated datetime,
	@dateSolved datetime = '1900-01-01 00:00:00.000'
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    declare @repositoryId int
	declare @assigneeId int
	declare @typeId int
	declare @statusId int

	/* Inserts a record into Repository if one isn't found */
	merge Repository as [target]
	using (select @repository as Repo) as [source] on [target].[Name] = [source].Repo
	when not matched then
		insert ([Name]) values ([source].Repo);

	select @repositoryId = RepositoryId from Repository with (nolock)
	where [Name] = @repository

	/* Inserts a record into User if one isn't found */
	merge [User] as [target]
	using (select @assignee as Username) as [source] on [target].Username = [source].Username
	when not matched then
		insert (Username) values ([source].Username);

	select @assigneeId = UserId from [User] with (nolock)
	where Username = @assignee

	/* Inserts a record into Type if one isn't found */
	merge [Type] as [target]
	using (select @type as IssueType) as [source] on [target].[Value] = [source].IssueType
	when not matched then
		insert ([Value]) values ([source].IssueType);

	select @typeId = TypeId from [Type] with (nolock)
	where [Value] = @type

	/* Inserts a record into Status if one isn't found */
	merge [Status] as [target]
	using (select @status as IssueStatus) as [source] on [target].[Value] = [source].IssueStatus
	when not matched then
		insert ([Value]) values ([source].IssueStatus);

	select @statusId = StatusId from [Status] with (nolock)
	where [Value] = @status

	/* Updates a record if one is found or inserts one if it isn't */
	merge Issue as [target]
	using (
		select
			@repositoryId as RepositoryId,
            @issueId as GitHubIssueId,
            @number as Number,
            @title as Title,
            @assigneeId as AssigneeId,
            @typeId as TypeId,
            @statusId as StatusId,
            @dateCreated as DateCreated,
            @dateSolved as DateSolved
	) as [source]
	on [target].GitHubIssueId = [source].GitHubIssueId
	when matched then
		update set
			Title = [source].Title,
			AssigneeId = [source].AssigneeId,
			TypeId = [source].TypeId,
			StatusId = [source].StatusId,
			DateSolved = [source].DateSolved
	when not matched then
		insert (RepositoryId, GitHubIssueId, Number, Title, AssigneeId, TypeId, StatusId, DateCreated, DateSolved)
        values ([source].RepositoryId, [source].GitHubIssueId, [source].Number, [source].Title, [source].AssigneeId, [source].TypeId, [source].StatusId, [source].DateCreated, [source].DateSolved);

END
GO


