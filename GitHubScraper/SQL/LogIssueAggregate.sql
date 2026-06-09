if exists (
	select IssueAggregateId from IssueAggregate with (nolock)
	where RepositoryId = (
		select RepositoryId from Repository with (nolock)
		where [Name] = @repository
	)
	and [Date] = @date
)
begin
	
	update IssueAggregate set Created = Created + @created, Solved = Solved + @solved
	where RepositoryId = (
		select RepositoryId from Repository with (nolock)
		where [Name] = @repository
	)
	and [Date] = @date

end
else
begin
	
	insert into IssueAggregate (RepositoryId, [Date], Created, Solved)
	values (
		(
			select RepositoryId from Repository with (nolock)
			where [Name] = @repository
		),
		@date,
		@created,
		@solved
	)

end