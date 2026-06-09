select GitHubIssueId, DateCreated, DateSolved from Issue with (nolock)
join Repository with (nolock) on Issue.RepositoryId = Repository.RepositoryId
where [Name] = @repository
order by IssueId asc