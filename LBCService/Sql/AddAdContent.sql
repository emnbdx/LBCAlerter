CREATE PROCEDURE AddAdContent (
	@ad_id	int,
	@type nvarchar(max),
	@value nvarchar(max)
)

AS

INSERT INTO AdContent (Type, Value, Ad_ID) VALUES (@type, @value, @ad_id)
