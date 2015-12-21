CREATE PROCEDURE GetLastAttempt (
	@search_id int
)

AS

SELECT MAX(ProcessDate)
FROM Attempts WITH(NOLOCK)
WHERE Search_ID = @search_id