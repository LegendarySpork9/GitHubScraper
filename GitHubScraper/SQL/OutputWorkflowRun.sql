DECLARE	@returnValue int

EXEC	@returnValue = [dbo].[StoreWorkflowRun]
		@repository,
		@workflow,
		@workflowRunId,
		@runNumber,
		@actor,
		@displayTitle,
		@event,
		@status,
		@conclusion,
		@dateCreated

SELECT	'Return Value' = @returnValue