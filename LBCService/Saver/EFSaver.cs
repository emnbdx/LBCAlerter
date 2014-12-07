namespace LBCService.Saver
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    using LBCAlerterWeb.Models;
    using LBCMapping;

    using Newtonsoft.Json.Linq;

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
        public bool Store(JToken ad)
        {
            var hash = GetMd5Hash(ad["Title"].ToString() + ad.SelectToken("Contents").Children().FirstOrDefault(x => x.Type.ToString() == "Description"));
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
                                  Value = ad.AllowCommercial.ToString()
                              };
            contents.Add(content);

            if (!string.IsNullOrEmpty(ad.ContactUrl))
            {
                content = new AdContent { Type = AdContent.ContentType.ContactUrl.ToString(), Value = ad.ContactUrl };
                contents.Add(content);
            }

            if (!string.IsNullOrEmpty(ad.Description))
            {
                content = new AdContent { Type = AdContent.ContentType.Description.ToString(), Value = ad.Description };
                contents.Add(content);
            }

            if (!string.IsNullOrEmpty(ad.Name))
            {
                content = new AdContent { Type = AdContent.ContentType.Name.ToString(), Value = ad.Name };
                contents.Add(content);
            }

            if (!string.IsNullOrEmpty(ad.Phone))
            {
                content = new AdContent { Type = AdContent.ContentType.Phone.ToString(), Value = ad.Phone };
                contents.Add(content);
            }

            if (!string.IsNullOrEmpty(ad.Place))
            {
                content = new AdContent { Type = AdContent.ContentType.Place.ToString(), Value = ad.Place };
                contents.Add(content);
            }

            if (!string.IsNullOrEmpty(ad.Price))
            {
                content = new AdContent { Type = AdContent.ContentType.Price.ToString(), Value = ad.Price };
                contents.Add(content);
            }

            if (ad.Param != null)
            {
                foreach (var param in ad.Param)
                {
                    content = new AdContent { Type = AdContent.ContentType.Param.ToString(), Value = param };
                    contents.Add(content);
                }
            }

            if (ad.PictureUrl != null)
            {
                foreach (var pictureUrl in ad.PictureUrl)
                {
                    content = new AdContent { Type = AdContent.ContentType.PictureUrl.ToString(), Value = pictureUrl };
                    contents.Add(content);
                }
            }

            var tmpAd = new LBCAlerterWeb.Models.Ad
                            {
                                Url = ad.AdUrl,
                                Hash = hash,
                                Date = ad.Date,
                                Title = ad.Title,
                                Search = s,
                                Contents = contents
                            };
            this.db.Ads.Add(tmpAd);

            this.db.SaveChanges();

            ad.Id = tmpAd.ID;
            ad.SearchId = this.searchId;
            return true;
        }

        /// <summary>
        /// The get md 5 hash.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string GetMd5Hash(string input)
        {
            using (var md5Hash = MD5.Create())
            {
                var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                var result = new StringBuilder();
                foreach (var t in data)
                {
                    result.Append(t.ToString("x2"));
                }

                return result.ToString();
            }
        }
    }
}
