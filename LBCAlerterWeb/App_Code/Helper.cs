
namespace LBCAlerterWeb
{
    using System.Collections.Generic;
    using System.Linq;

    using LBCAlerterWeb.Models;

    /// <summary>
    /// The helper.
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// The db.
        /// </summary>
        private static readonly ApplicationDbContext Db = new ApplicationDbContext();

        /// <summary>
        /// The get notification count.
        /// </summary>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static int GetNotificationCount(string userId)
        {
            return Db.Notifications.Count(notification => notification.User.Id == userId && !notification.Viewed);
        }

        /// <summary>
        /// The get important notifications.
        /// </summary>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>IEnumerable</cref>
        ///     </see>
        ///     .
        /// </returns>
        public static IEnumerable<Notification> GetImportantNotifications(string userId)
        {
            return Db.Notifications.Where(notification => notification.User.Id == userId && !notification.Viewed && notification.Important);
        }

        /// <summary>
        /// The set notification as read.
        /// </summary>
        /// <param name="notification">
        /// The notification.
        /// </param>
        public static void SetNotificationAsRead(Notification notification)
        {
            notification.Viewed = true;
            Db.SaveChanges();
        }

        /// <summary>
        /// The set all notification as read.
        /// </summary>
        /// <param name="userId">
        /// The user id.
        /// </param>
        public static void SetAllNotificationAsRead(string userId)
        {
            IEnumerable<Notification> notifications =
                Db.Notifications.Where(notification => notification.User.Id == userId && !notification.Viewed);
            foreach (var notification in notifications)
            {
                notification.Viewed = true;
            }

            Db.SaveChanges();
        }

        /// <summary>
        /// The get search.
        /// </summary>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>IEnumerable</cref>
        ///     </see>
        ///     .
        /// </returns>
        public static IEnumerable<Search> GetSearch(string userId)
        {
            return Db.Searches.Where(search => search.User.Id == userId).ToList();
        }
    }
}