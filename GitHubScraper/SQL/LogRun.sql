insert into RunHistory (RepositoryId, RunDate, Issues, Commits, PullRequests, WorkflowRuns, Releases)
values (
	(
		select RepositoryId from Repository with (nolock)
		where [Name] = @repository
	),
	@runDate,
	@issues,
	@commits,
	@pullRequests,
	@workflowRuns,
	@releases
)