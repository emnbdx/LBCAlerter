
namespace LBCAlerterWeb.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// The payment.
    /// </summary>
    [Table("Don")]
    public class Don
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the paypal id.
        /// </summary>
        public string PaypalId { get; set; }

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the update date.
        /// </summary>
        public DateTime UpdateDate { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the currency.
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        public virtual ApplicationUser User { get; set; }

        /// <summary>
        /// Gets or sets the payer id.
        /// </summary>
        public string PayerId { get; set; }

        /// <summary>
        /// Gets or sets the payer email.
        /// </summary>
        public string PayerEmail { get; set; }

        /// <summary>
        /// Gets or sets the payer first name.
        /// </summary>
        public string PayerFirstName { get; set; }

        /// <summary>
        /// Gets or sets the payer last name.
        /// </summary>
        public string PayerLastName { get; set; }
    }
}