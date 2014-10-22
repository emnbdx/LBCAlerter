
namespace LBCAlerterWeb.Models
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// The notification.
    /// </summary>
    [Table("Notification")]
    public class Notification
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        [Required]
        [DisplayName("Titre")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        [Required]
        [DisplayName("Message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether important.
        /// </summary>
        public bool Important { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether viewed.
        /// </summary>
        public bool Viewed { get; set; }

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        public virtual ApplicationUser User { get; set; }
    }
}