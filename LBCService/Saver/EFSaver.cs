namespace LBCService.Saver
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
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
            var url = (string)ad["Url"];
            var databaseAd = this.db.Ads.FirstOrDefault(entry => entry.Search.ID == this.searchId && entry.Url == url);

            // If ad have same url
            if (databaseAd != null)
            {
                return false;
            }

            var hash = (ad["Title"].ToString() + ad["Place"]).GetMd5Hash();
            databaseAd = this.db.Ads.FirstOrDefault(entry => entry.Search.ID == this.searchId && entry.Hash == hash);

            // If ad have same hash
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
                                  Value = ad.GetTokenValue("Contents>AllowCommercial").FirstOrDefault()
                              };
            contents.Add(content);

            content = new AdContent
                          {
                              Type = AdContent.ContentType.ContactUrl.ToString(),
                              Value = ad.GetTokenValue("Contents>ContactUrl").FirstOrDefault()
                          };
            contents.Add(content);

            content = new AdContent
                          {
                              Type = AdContent.ContentType.Description.ToString(),
                              Value = ad.GetTokenValue("Contents>Description").FirstOrDefault()
                          };
            contents.Add(content);

            content = new AdContent
                          {
                              Type = AdContent.ContentType.Name.ToString(),
                              Value = ad.GetTokenValue("Contents>Name").FirstOrDefault()
                          };
            contents.Add(content);

            content = new AdContent
                          {
                              Type = AdContent.ContentType.Phone.ToString(),
                              Value = ad.GetTokenValue("Contents>Phone").FirstOrDefault()
                          };
            contents.Add(content);

            content = new AdContent
                          {
                              Type = AdContent.ContentType.Place.ToString(),
                              Value = ad.GetTokenValue("Contents>Place").FirstOrDefault()
                          };
            contents.Add(content);

            content = new AdContent
                          {
                              Type = AdContent.ContentType.Price.ToString(),
                              Value = ad.GetTokenValue("Contents>Price").FirstOrDefault()
                          };
            contents.Add(content);

            foreach (var param in ad.GetTokenValue("Contents>Param"))
            {
                content = new AdContent { Type = AdContent.ContentType.Param.ToString(), Value = param };
                contents.Add(content);
            }

            foreach (var picture in ad.GetTokenValue("Contents>PictureUrl"))
            {
                content = new AdContent { Type = AdContent.ContentType.PictureUrl.ToString(), Value = picture };
                contents.Add(content);
            }

            var tmpAd = new Ad
                            {
                                Url = (string)ad["Url"],
                                Hash = hash,
                                Date = DateTime.Parse((string)ad["Date"], new CultureInfo("fr-FR")),
                                Title = (string)ad["Title"],
                                Search = s,
                                Contents = contents
                            };
            this.db.Ads.Add(tmpAd);

            this.db.SaveChanges();

            using (var writer = ad.CreateWriter())
            {
                writer.WritePropertyName("Id");
                writer.WriteValue(tmpAd.ID);
                writer.WritePropertyName("SearchId");
                writer.WriteValue(this.searchId);
            }

            return true;
        }
    }
}
