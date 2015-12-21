CREATE PROCEDURE AddAttempt (
	@search_id int,
	@count int
)

AS 

INSERT INTO Attempts (ProcessDate, AdsFound, Search_ID) VALUES (getdate(), @count, @search_id)