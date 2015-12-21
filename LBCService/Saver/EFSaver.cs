// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EFSaver.cs" company="Eddy MONTUS">
//   2014
// </copyright>
// <summary>
//   The entity framework saver.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Data.SqlClient;

namespace LBCService.Saver
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

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
            decimal adId;
            using (var db = new ApplicationDbContext())
            {
                var url = (string) ad["Url"];
                var databaseAd =
                    db.Ads.FirstOrDefault(entry => entry.Search.ID == this.searchId && entry.Url == url);

                // If ad have same url
                if (databaseAd != null)
                {
                    return false;
                }

                var uniqueId = ad["Title"] + ad.GetTokenValue("Contents>Place").FirstOrDefault()
                               + ad.GetTokenValue("Contents>Price").FirstOrDefault()
                               + ad.GetTokenValue("Contents>Description").FirstOrDefault();
                var hash = uniqueId.GetMd5Hash();
                databaseAd = db.Ads.FirstOrDefault(entry => entry.Search.ID == this.searchId && entry.Hash == hash);

                // If ad have same hash
                if (databaseAd != null)
                {
                    return false;
                }

                var s = db.Searches.FirstOrDefault(entry => entry.ID == this.searchId);
                if (s == null)
                {
                    throw new Exception("Recherche inexistante...");
                }

                adId = db.Database.SqlQuery<decimal>("exec AddAd @search_id, @url, @date, @title, @hash",
                    new SqlParameter("search_id", s.ID),
                    new SqlParameter("url", (string)ad["Url"]),
                    new SqlParameter("date", DateTime.Parse((string)ad["Date"], new CultureInfo("fr-FR"))),
                    new SqlParameter("title", (string)ad["Title"]),
                    new SqlParameter("hash", hash)).First();
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
                              Type = AdContent.ContentType.Latitude.ToString(),
                              Value = ad.GetTokenValue("Contents>Latitude").FirstOrDefault()
                          };
            contents.Add(content);

            content = new AdContent
                          {
                              Type = AdContent.ContentType.Longitude.ToString(),
                              Value = ad.GetTokenValue("Contents>Longitude").FirstOrDefault()
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


            using (var db = new ApplicationDbContext())
            {
                foreach (var adContent in contents.Where(c => c.Value != null))
                {
                    db.Database.ExecuteSqlCommand("exec AddAdContent @ad_id, @type, @value",
                        new SqlParameter("ad_id", (int)adId),
                        new SqlParameter("type", adContent.Type),
                        new SqlParameter("value", adContent.Value));
                }
            }

            using (var writer = ad.CreateWriter())
            {
                writer.WritePropertyName("Id");
                writer.WriteValue(adId);
                writer.WritePropertyName("SearchId");
                writer.WriteValue(this.searchId);
            }

            return true;
        }
    }
}
