namespace LBCService.Saver
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
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
        public bool Store(JObject ad)
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

            var tmp = ad["Contents"].Children().FirstOrDefault(entry => entry["Type"].ToString() == "AllowCommercial");
            if (tmp != null)
            {
                var allowCommercial = (string)tmp["Value"];
                var content = new AdContent
                                  {
                                      Type = AdContent.ContentType.AllowCommercial.ToString(),
                                      Value = allowCommercial
                                  };
                contents.Add(content);
            }

            tmp = ad["Contents"].Children().FirstOrDefault(entry => entry["Type"].ToString() == "ContactUrl");
            if (tmp != null)
            {
                var contactUrl = (string)tmp["Value"];
                if (!string.IsNullOrEmpty(contactUrl))
                {
                    var content = new AdContent { Type = AdContent.ContentType.ContactUrl.ToString(), Value = contactUrl };
                    contents.Add(content);
                }
            }

            tmp = ad["Contents"].Children().FirstOrDefault(entry => entry["Type"].ToString() == "Description");
            if (tmp != null)
            {
                var description = (string)tmp["Value"];
                if (!string.IsNullOrEmpty(description))
                {
                    var content = new AdContent { Type = AdContent.ContentType.Description.ToString(), Value = description };
                    contents.Add(content);
                }
            }

            tmp = ad["Contents"].Children().FirstOrDefault(entry => entry["Type"].ToString() == "Name");
            if (tmp != null)
            {
                var name = (string)tmp["Value"];
                if (!string.IsNullOrEmpty(name))
                {
                    var content = new AdContent { Type = AdContent.ContentType.Name.ToString(), Value = name };
                    contents.Add(content);
                }
            }

            tmp = ad["Contents"].Children().FirstOrDefault(entry => entry["Type"].ToString() == "Phone");
            if (tmp != null)
            {
                var phone = (string)tmp["Value"];
                if (!string.IsNullOrEmpty(phone))
                {
                    var content = new AdContent { Type = AdContent.ContentType.Phone.ToString(), Value = phone };
                    contents.Add(content);
                }
            }

            tmp = ad["Contents"].Children().FirstOrDefault(entry => entry["Type"].ToString() == "Place");
            if (tmp != null)
            {
                var place = (string)tmp["Value"];
                if (!string.IsNullOrEmpty(place))
                {
                    var content = new AdContent { Type = AdContent.ContentType.Place.ToString(), Value = place };
                    contents.Add(content);
                }
            }

            tmp = ad["Contents"].Children().FirstOrDefault(entry => entry["Type"].ToString() == "Price");
            if (tmp != null)
            {
                var price = (string)tmp["Value"];
                if (!string.IsNullOrEmpty(price))
                {
                    var content = new AdContent { Type = AdContent.ContentType.Price.ToString(), Value = price };
                    contents.Add(content);
                }
            }

            var tmps = ad["Contents"].Children().Where(entry => entry["Type"].ToString() == "Param");
            if (tmps != null)
            {
                foreach (var t in tmps)
                {
                    var content = new AdContent { Type = AdContent.ContentType.Param.ToString(), Value = (string)t["Value"] };
                    contents.Add(content);
                }
            }

            tmps = ad["Contents"].Children().Where(entry => entry["Type"].ToString() == "PictureUrl");
            if (tmps != null)
            {
                foreach (var t in tmps)
                {
                    var content = new AdContent { Type = AdContent.ContentType.PictureUrl.ToString(), Value = (string)t["Value"] };
                    contents.Add(content);
                }
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
