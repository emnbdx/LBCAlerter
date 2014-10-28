namespace LBCService.Counter
{
    using System;
    using System.Linq;

    using LBCAlerterWeb.Models;
    using LBCMapping;

    /// <summary>
    /// The ef counter.
    /// </summary>
    public class EfCounter : ICounter
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly ApplicationDbContext db = new ApplicationDbContext();

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
            var search = this.db.Searches.FirstOrDefault(entry => entry.ID == this.searchId);

            if (search != null)
            {
                search.Attempts.Add(
                    new Attempt
                        {
                            ProcessDate = DateTime.Now,
                            AdsFound = count,
                            Search = search
                        });
            }

            this.db.SaveChanges();
        }
    }
}
