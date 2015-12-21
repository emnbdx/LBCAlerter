GO
CREATE NONCLUSTERED INDEX [Attempts_ProcessDate_Search_ID]
ON [dbo].[Attempts] ([ProcessDate])
INCLUDE ([Search_ID])

GO
CREATE PROCEDURE GetAllSearch

AS

SELECT s.ID, 
		s.Url, 
		s.KeyWord, 
		s.MailAlert,
		s.MailRecap,
		s.LastRecap,
		s.RefreshTime,
		s.Enabled,
		u.UserName, 
		CASE WHEN r.RoleId is null THEN convert(bit, 0) ELSE convert(bit, 1) END as IsPremiumUser,
		(SELECT count(1) FROM Attempts WITH(NOLOCK) WHERE Search_ID = s.ID and ProcessDate > dateadd(day, -1, getdate())) as TodayAttempsCount,
		(SELECT count(1) FROM Ad WITH(NOLOCK) WHERE Search_ID = s.ID) as AdsCount
FROM Search s WITH(NOLOCK)
INNER JOIN AspNetUsers u WITH(NOLOCK) on u.ID = s.User_Id
LEFT JOIN AspNetUserRoles r WITH(NOLOCK) on r.UserId = u.ID and r.RoleId = (SELECT Id 
																			FROM AspNetRoles WITH(NOLOCK) 
																			WHERE Name = 'premium')


				