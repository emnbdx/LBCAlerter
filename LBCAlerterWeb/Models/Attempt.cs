using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LBCAlerterWeb.Models
{
    public class Attempt
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public DateTime ProcessDate { get; set; }
        public Int32 AdsFound { get; set; }
        
        public virtual Search Search { get; set; }
    }
}