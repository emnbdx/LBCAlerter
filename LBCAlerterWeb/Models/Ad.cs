
namespace LBCAlerterWeb.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// The ad.
    /// </summary>
    [Table("Ad")]
    public class Ad
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the url.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the picture url.
        /// </summary>
        public string PictureUrl { get; set; }

        /// <summary>
        /// Gets or sets the place.
        /// </summary>
        public string Place { get; set; }

        /// <summary>
        /// Gets or sets the price.
        /// </summary>
        public string Price { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the phone.
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether allow commercial.
        /// </summary>
        public bool AllowCommercial { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the contact url.
        /// </summary>
        public string ContactUrl { get; set; }

        /// <summary>
        /// Gets or sets the param.
        /// </summary>
        public string Param { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the search.
        /// </summary>
        public virtual Search Search { get; set; }

        /// <summary>
        /// The convert lbc ad.
        /// </summary>
        /// <param name="ad">
        /// The ad.
        /// </param>
        /// <returns>
        /// The <see cref="Ad"/>.
        /// </returns>
        public static Ad ConvertLbcAd(LBCMapping.Ad ad)
        {
            return new Ad
                       {
                           Url = ad.AdUrl,
                           Date = ad.Date,
                           PictureUrl = ad.PictureUrl,
                           Place = ad.Place,
                           Price = ad.Price,
                           Title = ad.Title,
                           Phone = ad.Phone,
                           AllowCommercial = ad.AllowCommercial,
                           Name = ad.Name,
                           ContactUrl = ad.ContactUrl,
                           Param = ad.Param,
                           Description = ad.Description
                       };
        }
    }
}