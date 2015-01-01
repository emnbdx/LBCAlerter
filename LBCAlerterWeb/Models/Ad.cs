// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Ad.cs" company="Eddy MONTUS">
//   2014
// </copyright>
// <summary>
//   The ad.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LBCAlerterWeb.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using Newtonsoft.Json;

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
        /// Gets or sets the hash.
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the search.
        /// </summary>
        [JsonIgnore]
        public virtual Search Search { get; set; }

        /// <summary>
        /// Gets or sets the contents.
        /// </summary>
        public virtual ICollection<AdContent> Contents { get; set; } 
    }
}