CREATE PROCEDURE AddAd (
	@search_id	int,
	@url nvarchar(max),
	@date datetime,
	@title nvarchar(max),
	@hash nvarchar(max)
)

AS

INSERT INTO Ad (Url, Date, Title, Search_ID, Hash) VALUES (@url, @date, @title, @search_id, @hash)

SELECT SCOPE_IDENTITY()