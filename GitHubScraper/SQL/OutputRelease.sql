DECLARE	@returnValue int

EXEC	@returnValue = [dbo].[StoreRelease]
		@repository,
		@releaseId,
		@name,
		@author,
		@draft,
		@assets,
		@body,
		@dateCreated,
		@datePublished

SELECT	'Return Value' = @returnValue