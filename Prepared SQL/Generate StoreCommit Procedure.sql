USE [GitHub]
GO

/****** Object:  StoredProcedure [dbo].[StoreCommit]    Script Date: 05/09/2025 10:56:20 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Toby Hunter
-- Create date: 05/09/2025
-- Description:	Procedure for logging commits collected from GitHub.
-- =============================================
CREATE PROCEDURE [dbo].[StoreCommit] 
    @repository varchar(50),
    @author varchar(50),
    @committer varchar(50),
	@sha varchar(50),
    @message varchar(max)
AS
BEGIN
    SET NOCOUNT ON;

    declare @repositoryId int;
    declare @authorId int;
    declare @committerId int;

    /* Inserts a record into Repository if one isn't found */
	merge Repository as [target]
	using (select @repository as Repo) as [source] on [target].[Name] = [source].Repo
	when not matched then
		insert ([Name]) values ([source].Repo);
	
	select @repositoryId = RepositoryId from Repository with (nolock)
	where [Name] = @repository

    /* Inserts a record into User if one isn't found (author) */
	merge [User] as [target]
	using (select @author as Username) as [source] on [target].Username = [source].Username
	when not matched then
		insert (Username) values ([source].Username);
		
	select @authorId = UserId from [User] with (nolock)
	where Username = @author

	/* Inserts a record into User if one isn't found (committer) */
	merge [User] as [target]
	using (select @committer as Username) as [source] on [target].Username = [source].Username
	when not matched then
		insert (Username) values ([source].Username);
		
	select @committerId = UserId from [User] with (nolock)
	where Username = @committer

	/* Updates a record if one is found or inserts one if it isn't */
	merge [Commit] as [target]
	using (
		select
			@repositoryId as RepositoryId,
            @sha as GitHubCommitId,
            @authorId as AuthorId,
            @committerId as CommitterId,
            @message as [Message]
	) as [source]
	on [target].GitHubCommitId = [source].GitHubCommitId
	when matched then
		update set
			AuthorId = [source].AuthorId,
			CommitterId = [source].CommitterId,
			[Message] = [source].[Message]
	when not matched then
		insert (RepositoryId, AuthorId, CommitterId, GitHubCommitId, [Message])
        values ([source].RepositoryId, [source].AuthorId, [source].CommitterId, [source].GitHubCommitId, [source].[Message]);

END
GO


