DECLARE	@returnValue int

EXEC	@returnValue = [dbo].[StoreCommit]
		@repository,
		@author,
		@committer,
		@sha,
		@message

SELECT	'Return Value' = @returnValue