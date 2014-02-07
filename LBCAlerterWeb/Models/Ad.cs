using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LBCAlerterWeb.Models
{
    [Table("Ad")]
    public class Ad
    {
        public int ID { get; set; }
        public String Url { get; set;}
        public virtual Search Search { get; set; }
    }
}