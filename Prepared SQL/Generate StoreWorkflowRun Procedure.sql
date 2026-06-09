USE [GitHub]
GO

/****** Object:  StoredProcedure [dbo].[StoreWorkflowRun]    Script Date: 05/09/2025 16:19:41 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Toby Hunter
-- Create date: 05/09/2025
-- Description:	Procedure for logging workflow runs collected from GitHub.
-- =============================================
CREATE PROCEDURE [dbo].[StoreWorkflowRun] 
	@repository varchar(50),
	@workflow varchar(255),
	@workflowRunId bigint,
	@runNumber int,
	@actor varchar(50),
	@displayTitle varchar(80),
	@event varchar(255),
	@status varchar(20),
	@conclusion varchar(20),
	@dateCreated datetime
as
begin
	-- set nocount on added to prevent extra result sets from
	-- interfering with select statements.
	set nocount on;

    declare @repositoryId int
	declare @actorId int
	declare @eventId int
	declare @workflowId int
	declare @statusId int
	declare @conclusionId int

	/* Inserts a record into Repository if one isn't found */
	merge Repository as [target]
	using (select @repository as Repo) as [source] on [target].[Name] = [source].Repo
	when not matched then
		insert ([Name]) values ([source].Repo);

	select @repositoryId = RepositoryId from Repository with (nolock)
	where [Name] = @repository

	/* Inserts a record into User if one isn't found */
	merge [User] as [target]
	using (select @actor as Username) as [source] on [target].Username = [source].Username
	when not matched then
		insert (Username) values ([source].Username);

	select @actorId = UserId from [User] with (nolock)
	where Username = @actor

	/* Inserts a record into Event if one isn't found */
	merge [Event] as [target]
	using (select @event as RunEvent) as [source] on [target].[Name] = [source].RunEvent
	when not matched then
		insert ([Name]) values ([source].RunEvent);

	select @eventId = EventId from [Event] with (nolock)
	where [Name] = @event

	/* Inserts a record into Workflow if one isn't found */
	merge Workflow as [target]
	using (select @workflow as WorkflowName) as [source] on [target].[Name] = [source].WorkflowName
	when not matched then
		insert ([Name]) values ([source].WorkflowName);

	select @workflowId = WorkflowId from Workflow with (nolock)
	where [Name] = @workflow

	/* Inserts a record into Status if one isn't found (Status) */
	merge [Status] as [target]
	using (select @status as RunStatus) as [source] on [target].[Value] = [source].RunStatus
	when not matched then
		insert ([Value]) values ([source].RunStatus);

	select @statusId = StatusId from [Status] with (nolock)
	where [Value] = @status

	/* Inserts a record into Status if one isn't found (Conclusion) */
	merge [Status] as [target]
	using (select @conclusion as RunConclusion) as [source] on [target].[Value] = [source].RunConclusion
	when not matched then
		insert ([Value]) values ([source].RunConclusion);

	select @conclusionId = StatusId from [Status] with (nolock)
	where [Value] = @conclusion

	/* updates a record if one is found or inserts one if it isn't */
	merge WorkflowRun as [target]
	using (
		select
			@repositoryId as RepositoryId,
			@workflowRunId as GitHubRunId,
			@runNumber as RunNumber,
			@actorId as ActorId,
			@displayTitle as DisplayTitle,
			@eventId as [Event],
			@workflowId as Workflow,
			@statusId as [Status],
			@conclusionId as Conclusion,
			@dateCreated as DateCreated
	) as [source]
	on [target].GitHubRunId = [source].GitHubRunId
	when matched then
		update set
			StatusId = [source].[Status],
			ConclusionId = [source].Conclusion
	when not matched then
		insert (RepositoryId, ActorId, EventId, WorkflowId, StatusId, ConclusionId, GitHubRunId, Title, RunNumber, DateCreated)
		values ([source].RepositoryId, [source].ActorId, [source].[Event], [source].Workflow, [source].[Status], [source].Conclusion, [source].GitHubRunId, [source].DisplayTitle, [source].RunNumber, [source].DateCreated);

END
GO


