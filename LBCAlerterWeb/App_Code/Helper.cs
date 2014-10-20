
namespace LBCAlerterWeb
{
    using System.Collections.Generic;
    using System.Linq;

    using LBCAlerterWeb.Models;

    public static class Helper
    {
        private static ApplicationDbContext db = new ApplicationDbContext();
        
        public static int GetNotificationCount(string userId)
        {
            return db.Notifications.Count(notification => notification.User.Id == userId && !notification.Viewed);
        }

        public static IEnumerable<Notification> GetImportantNotifications(string userId)
        {
            return db.Notifications.Where(notification => notification.User.Id == userId && !notification.Viewed && notification.Important);
        }

        public static IEnumerable<Search> GetSearch(string userId)
        {
            return db.Searches.Where(search => search.User.Id == userId).ToList();
        }
    }
}