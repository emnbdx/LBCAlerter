using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public int ID { get; set; }
        [DisplayName("Url de recherche")]
        public string Url { get; set; }
        [DisplayName("Description")]
        public string KeyWord { get; set; }
        [DisplayName("Alertes email ?")]
        public bool MailAlert { get; set; }
        [DisplayName("Récap. email ?")]
        public bool MailRecap { get; set; }
        public DateTime LastRecap { get; set; }
        [DisplayName("Actualisation (min)")]
        [Range(5, 1440, ErrorMessage="Le temps d'actualisation doit être compris en 5 et 1440 minutes")]
        public int RefreshTime { get; set; }
        public virtual ApplicationUser User { get; set; }
        [DisplayName("Annonces")]
        public virtual List<Ad> Ads { get; set; }
    }
}