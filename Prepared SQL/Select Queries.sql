/* Issues */
select IssueId, [Name], Username, [Type].[Value], [Status].[Value], GitHubIssueId, Title, Number, DateCreated, DateSolved from Issue with (nolock)
join Repository with (nolock) on Issue.RepositoryId = Repository.RepositoryId
join [User] with (nolock) on Issue.AssigneeId = [User].UserId
join [Type] with (nolock) on Issue.TypeId = [Type].TypeId
join [Status] with (nolock) on Issue.StatusId = [Status].StatusId
order by [Name], Number asc

/* Commits */
select CommitId, [Name], A.Username, C.Username, GitHubCommitId, [Message] from [Commit] with (nolock)
join Repository with (nolock) on [Commit].RepositoryId = Repository.RepositoryId
join [User] A with (nolock) on [Commit].AuthorId = A.UserId
join [User] C with (nolock) on [Commit].CommitterId = C.UserId

/* Pull Requests */
select PullRequestId, [Name], Username, [Type].[Value], [Status].[Value], GitHubPullRequestId, Title, Number, DateCreated, DateSolved, DateMerged from PullRequest with (nolock)
join Repository with (nolock) on PullRequest.RepositoryId = Repository.RepositoryId
join [User] with (nolock) on PullRequest.AssigneeId = [User].UserId
join [Type] with (nolock) on PullRequest.TypeId = [Type].TypeId
join [Status] with (nolock) on PullRequest.StatusId = [Status].StatusId
order by [Name], Number asc

/* Workflow Runs */
select RunId, Repository.[Name], Username, [Event].[Name], Workflow.[Name], S.[Value], C.[Value], GitHubRunId, Title, RunNumber, DateCreated from WorkflowRun with (nolock)
join Repository with (nolock) on WorkflowRun.RepositoryId = Repository.RepositoryId
join [User] with (nolock) on WorkflowRun.ActorId = [User].UserId
join [Event] with (nolock) on WorkflowRun.EventId = [Event].EventId
join Workflow with (nolock) on WorkflowRun.WorkflowId = Workflow.WorkflowId
join [Status] S with (nolock) on WorkflowRun.StatusId = S.StatusId
join [Status] C with (nolock) on WorkflowRun.ConclusionId = C.StatusId
order by Workflow.[Name], RunNumber asc

/* Releases */
select ReleaseId, Repository.[Name], Username, GitHubReleaseId, Release.[Name], Draft, NumberOfAssets, Body, DateCreated, DatePublished from Release with (nolock)
join Repository with (nolock) on Release.RepositoryId = Repository.RepositoryId
join [User] with (nolock) on Release.AuthorId = [User].UserId

/* Issue Aggregates */
select IssueAggregateId, [Name], [Date], Created, Solved from IssueAggregate with (nolock)
join Repository with (nolock) on IssueAggregate.RepositoryId = Repository.RepositoryId
order by [Name], [Date] asc

/* Run History */
select RunHistoryId, [Name], RunDate, Issues, Commits, PullRequests, WorkflowRuns, Releases from RunHistory with (nolock)
join Repository with (nolock) on RunHistory.RepositoryId = Repository.RepositoryId