insert into RunHistory (RepositoryId, RunDate, Issues, Commits, PullRequests, WorkflowRuns, Releases)
values (
	(
		select RepositoryId from Repository with (nolock)
		where [Name] = @repository
	),
	getutcdate(),
	@issues,
	@commits,
	@pullRequests,
	@workflowRuns,
	@releases
)