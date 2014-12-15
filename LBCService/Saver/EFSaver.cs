namespace LBCService.Saver
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    using EMToolBox;

    using LBCAlerterWeb.Models;
    using LBCMapping;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The entity framework saver.
    /// </summary>
    public class EfSaver : ISaver
    {
        /// <summary>
        /// The database context.
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
        public bool Store(JObject ad)
        {
            var hash = (ad["Title"] + ad.GetTagValue("Contents>Description").FirstOrDefault()).GetMd5Hash();
            var databaseAd = this.db.Ads.FirstOrDefault(entry => entry.Search.ID == this.searchId && entry.Hash == hash);

            if (databaseAd != null)
            {
                return false;
            }

            var s = this.db.Searches.FirstOrDefault(entry => entry.ID == this.searchId);
            if (s == null)
            {
                throw new Exception("Recherche inexistante...");
            }

            var contents = new List<AdContent>();

            var content = new AdContent
                              {
                                  Type = AdContent.ContentType.AllowCommercial.ToString(),
                                  Value = ad.GetTagValue("Contents>AllowCommercial").FirstOrDefault()
                              };
            contents.Add(content);

            content = new AdContent
                          {
                              Type = AdContent.ContentType.ContactUrl.ToString(),
                              Value = ad.GetTagValue("Contents>ContactUrl").FirstOrDefault()
                          };
            contents.Add(content);

            content = new AdContent
                          {
                              Type = AdContent.ContentType.Description.ToString(),
                              Value = ad.GetTagValue("Contents>Description").FirstOrDefault()
                          };
            contents.Add(content);

            content = new AdContent
                          {
                              Type = AdContent.ContentType.Name.ToString(),
                              Value = ad.GetTagValue("Contents>Name").FirstOrDefault()
                          };
            contents.Add(content);

            content = new AdContent
                          {
                              Type = AdContent.ContentType.Phone.ToString(),
                              Value = ad.GetTagValue("Contents>Phone").FirstOrDefault()
                          };
            contents.Add(content);

            content = new AdContent
                          {
                              Type = AdContent.ContentType.Place.ToString(),
                              Value = ad.GetTagValue("Contents>Place").FirstOrDefault()
                          };
            contents.Add(content);

            content = new AdContent
                          {
                              Type = AdContent.ContentType.Price.ToString(),
                              Value = ad.GetTagValue("Contents>Price").FirstOrDefault()
                          };
            contents.Add(content);

            foreach (var param in ad.GetTagValue("Contents>Param"))
            {
                content = new AdContent { Type = AdContent.ContentType.Param.ToString(), Value = param };
                contents.Add(content);
            }

            foreach (var picture in ad.GetTagValue("Contents>PictureUrl"))
            {
                content = new AdContent { Type = AdContent.ContentType.PictureUrl.ToString(), Value = picture };
                contents.Add(content);
            }

            var tmpAd = new Ad
                            {
                                Url = (string)ad["AdUrl"],
                                Hash = hash,
                                Date = (DateTime)ad["Date"],
                                Title = (string)ad["Title"],
                                Search = s,
                                Contents = contents
                            };
            this.db.Ads.Add(tmpAd);

            this.db.SaveChanges();

            /*ad.Id = tmpAd.ID;
            ad.SearchId = this.searchId;*/
            return true;
        }
    }
}
