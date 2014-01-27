using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LBCAlerterWeb.Models
{
    [Table("Search")]
    public class Search
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int SearchId { get; set; }
        public string Url { get; set; }
        public String KeyWord { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}