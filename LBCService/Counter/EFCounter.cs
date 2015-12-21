using System.Data.SqlClient;

namespace LBCService.Counter
{
    using LBCAlerterWeb.Models;
    using LBCMapping;

    /// <summary>
    /// The ef counter.
    /// </summary>
    public class EfCounter : ICounter
    {
        /// <summary>
        /// The m_search id.
        /// </summary>
        private readonly int searchId;

        /// <summary>
        /// Initializes a new instance of the <see cref="EfCounter"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        public EfCounter(int id)
        {
            this.searchId = id;
        }

        /// <summary>
        /// The count.
        /// </summary>
        public void Count()
        {
            // Nothing to do
        }

        /// <summary>
        /// The result.
        /// </summary>
        /// <param name="count">
        /// The count.
        /// </param>
        public void Result(int count)
        {
            using (var db = new ApplicationDbContext())
            {
                db.Database.ExecuteSqlCommand("exec AddAttempt @search_id, @count",
                    new SqlParameter("search_id", this.searchId), new SqlParameter("count", count));
            }
        }
    }
}
