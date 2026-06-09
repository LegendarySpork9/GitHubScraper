DECLARE	@returnValue int

EXEC	@returnValue = [dbo].[StorePullRequest]
		@repository,
		@pullRequestId,
		@number,
		@title,
		@assignee,
		@type,
		@status,
		@dateCreated,
		@dateSolved,
		@dateMerged

SELECT	'Return Value' = @returnValue