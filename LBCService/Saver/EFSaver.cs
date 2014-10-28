namespace LBCService.Saver
{
    using System;
    using System.Linq;

    using LBCAlerterWeb.Models;
    using LBCMapping;

    /// <summary>
    /// The ef saver.
    /// </summary>
    public class EfSaver : ISaver
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// The search id.
        /// </summary>
        private readonly int searchId;

        /// <summary>
        /// Initializes a new instance of the <see cref="EfSaver"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        public EfSaver(int id)
        {
            this.searchId = id;
        }

        /// <summary>
        /// The store.
        /// </summary>
        /// <param name="ad">
        /// The ad.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// If search doesn't exist
        /// </exception>
        public bool Store(LBCMapping.Ad ad)
        {
            var databaseAd = this.db.Ads.FirstOrDefault(entry => entry.Search.ID == this.searchId && entry.Url == ad.AdUrl);

            if (databaseAd != null)
            {
                return false;
            }

            var tmpAd = LBCAlerterWeb.Models.Ad.ConvertLbcAd(ad);
            var s = this.db.Searches.FirstOrDefault(entry => entry.ID == this.searchId);
            if (s == null)
            {
                throw new Exception("Recherche inexistante...");
            }

            tmpAd.Search = s;
            this.db.Ads.Add(tmpAd);

            this.db.SaveChanges();
            return true;
        }
    }
}
