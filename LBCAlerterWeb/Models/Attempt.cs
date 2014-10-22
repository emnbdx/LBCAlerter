
namespace LBCAlerterWeb.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// The attempt.
    /// </summary>
    public class Attempt
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the process date.
        /// </summary>
        public DateTime ProcessDate { get; set; }

        /// <summary>
        /// Gets or sets the ads found.
        /// </summary>
        public int AdsFound { get; set; }

        /// <summary>
        /// Gets or sets the search.
        /// </summary>
        public virtual Search Search { get; set; }
    }
}