// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AdContent.cs" company="Eddy MONTUS">
//   2014
// </copyright>
// <summary>
//   The ad content.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LBCAlerterWeb.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// The ad content.
    /// </summary>
    [Table("AdContent")]
    public class AdContent
    {
        /// <summary>
        /// The content type.
        /// </summary>
        public enum ContentType
        {
            /// <summary>
            /// The picture url.
            /// </summary>
            PictureUrl,

            /// <summary>
            /// The place.
            /// </summary>
            Place,

            /// <summary>
            /// The price.
            /// </summary>
            Price,

            /// <summary>
            /// The phone.
            /// </summary>
            Phone,

            /// <summary>
            /// The allow commercial.
            /// </summary>
            AllowCommercial,

            /// <summary>
            /// The name.
            /// </summary>
            Name,

            /// <summary>
            /// The contact url.
            /// </summary>
            ContactUrl,

            /// <summary>
            /// The param.
            /// </summary>
            Param,

            /// <summary>
            /// The description.
            /// </summary>
            Description,

            /// <summary>
            /// The latitude.
            /// </summary>
            Latitude,

            /// <summary>
            /// The longitude.
            /// </summary>
            Longitude,
        }
        
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the ad.
        /// </summary>
        public virtual Ad Ad { get; set; }
    }
}