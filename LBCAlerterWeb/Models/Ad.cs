using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LBCAlerterWeb.Models
{
    [Table("Ad")]
    public class Ad
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public String Url { get; set;}
        public DateTime Date { get; set; }
        public string PictureUrl { get; set; }
        public string Place { get; set; }
        public string Price { get; set; }
        public string Title { get; set; }
        public string Phone { get; set; }
        public bool AllowCommercial { get; set; }
        public string Name { get; set; }
        public string ContactUrl { get; set; }
        public string Param { get; set; }
        public string Description { get; set; }
        
        public virtual Search Search { get; set; }

        public static Ad ConvertLBCAd(LBCMapping.Ad ad)
        {
            return new Ad()
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