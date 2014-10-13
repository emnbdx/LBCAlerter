
namespace LBCAlerterWeb.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Search")]
    public class Search
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        [DisplayName("Date de création")]
        public DateTime CreationDate { get; set; }
        [DisplayName("Url de recherche")]
        public string Url { get; set; }
        [DisplayName("Description")]
        public string KeyWord { get; set; }
        [Display(Name = " ")]
        public bool MailAlert { get; set; }
        [Display(Name = " ")]
        public bool MailRecap { get; set; }
        [DisplayName("Dernier récap")]
        public DateTime? LastRecap { get; set; }
        [DisplayName("Actualisation (min)")]
        [Range(5, 1440, ErrorMessage="Le temps d'actualisation doit être compris en 5 et 1440 minutes")]
        public int RefreshTime { get; set; }
        
        public virtual ApplicationUser User { get; set; }
        [DisplayName("Annonces")]
        public virtual List<Ad> Ads { get; set; }
        [DisplayName("Tentatives")]
        public virtual List<Attempt> Attempts { get; set; }
    }
}