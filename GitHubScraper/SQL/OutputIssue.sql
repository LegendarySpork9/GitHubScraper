DECLARE	@returnValue int

EXEC	@returnValue = [dbo].[StoreIssue]
		@repository,
		@issueId,
		@number,
		@title,
		@assignee,
		@type,
		@status,
		@dateCreated,
		@dateSolved

SELECT	'Return Value' = @returnValue