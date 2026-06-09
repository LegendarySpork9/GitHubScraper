USE [GitHub]
GO

/****** Object:  StoredProcedure [dbo].[StoreRelease]    Script Date: 09/09/2025 14:06:11 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Toby Hunter
-- Create date: 05/09/2025
-- Description:	Procedure for logging releases collected from GitHub.
-- =============================================
CREATE PROCEDURE [dbo].[StoreRelease] 
	@repository varchar(50),
	@releaseId bigint,
	@name varchar(80),
	@author varchar(50),
	@draft bit,
	@assets int,
	@body varchar(max),
	@dateCreated datetime,
	@datePublished datetime = '1900-01-01 00:00:00.000'
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    declare @repositoryId int
	declare @authorId int

	/* Inserts a record into Repository if one isn't found */
	merge Repository as [target]
	using (select @repository as Repo) as [source] on [target].[Name] = [source].Repo
	when not matched then
		insert ([Name]) values ([source].Repo);

	select @repositoryId = RepositoryId from Repository with (nolock)
	where [Name] = @repository

	/* Inserts a record into User if one isn't found */
	merge [User] as [target]
	using (select @author as Username) as [source] on [target].Username = [source].Username
	when not matched then
		insert (Username) values ([source].Username);

	select @authorId = UserId from [User] with (nolock)
	where Username = @author

	/* Updates a record if one is found or inserts one if it isn't */
	merge Release as [target]
	using (
		select
			@repositoryId AS RepositoryId,
            @releaseId AS GitHubReleaseId,
            @name AS [Name],
            @authorId AS AuthorId,
            @draft AS Draft,
            @assets AS Assets,
			@body as Body,
            @dateCreated AS DateCreated,
            @datePublished AS DatePublished
	) as [source]
	on [target].GitHubReleaseId = [source].GitHubReleaseId
	when matched then
		update set
			[Name] = [source].[Name],
			AuthorId = [source].AuthorId,
			Draft = [source].Draft,
			NumberOfAssets = [source].Assets,
			Body = [source].Body,
			DatePublished = [source].DatePublished
	when not matched then
		INSERT (RepositoryId, GithubReleaseId, AuthorId, [Name], Draft, NumberOfAssets, Body, DateCreated, DatePublished)
        VALUES ([source].RepositoryId, [source].GitHubReleaseId, [source].AuthorId, [source].[Name], [source].Draft, [source].Assets, [source].Body, [source].DateCreated, [source].DatePublished);

END
GO


