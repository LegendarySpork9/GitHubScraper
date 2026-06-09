select top 1 isnull(RunDate, '1900-01-01 00:00:00.000') from RunHistory with (nolock)
join Repository with (nolock) on RunHistory.RepositoryId = Repository.RepositoryId
where [Name] = @repository
order by RunHistoryId desc