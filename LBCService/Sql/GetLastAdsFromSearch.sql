CREATE PROCEDURE GetLastAdsFromSearch (
	@search_id int
)

AS

SELECT *
FROM Ad WITH(NOLOCK)
WHERE Search_ID = @search_id