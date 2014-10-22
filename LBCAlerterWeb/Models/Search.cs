
namespace LBCAlerterWeb.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// The search.
    /// </summary>
    [Table("Search")]
    public class Search
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        [DisplayName("Date de création")]
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the url.
        /// </summary>
        [DisplayName("Url de recherche")]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the key word.
        /// </summary>
        [DisplayName("Description")]
        public string KeyWord { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether mail alert.
        /// </summary>
        [Display(Name = " ")]
        public bool MailAlert { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether mail recap.
        /// </summary>
        [Display(Name = " ")]
        public bool MailRecap { get; set; }

        /// <summary>
        /// Gets or sets the last recap.
        /// </summary>
        [DisplayName("Dernier récap")]
        public DateTime? LastRecap { get; set; }

        /// <summary>
        /// Gets or sets the refresh time.
        /// </summary>
        [DisplayName("Actualisation (min)")]
        [Range(5, 1440, ErrorMessage = "Le temps d'actualisation doit être compris en 5 et 1440 minutes")]
        public int RefreshTime { get; set; }

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        public virtual ApplicationUser User { get; set; }

        /// <summary>
        /// Gets or sets the ads.
        /// </summary>
        [DisplayName("Annonces")]
        public virtual ICollection<Ad> Ads { get; set; }

        /// <summary>
        /// Gets or sets the attempts.
        /// </summary>
        [DisplayName("Tentatives")]
        public virtual ICollection<Attempt> Attempts { get; set; }
    }
}