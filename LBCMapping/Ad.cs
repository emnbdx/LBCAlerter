namespace LBCMapping
{
    using System;

    /// <summary>
    /// The ad.
    /// </summary>
    public class Ad
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the ad id.
        /// </summary>
        public int SearchId { get; set; }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the ad url.
        /// </summary>
        public string AdUrl { get; set; }

        /// <summary>
        /// Gets or sets the picture url.
        /// </summary>
        public string PictureUrl { get; set; }

        /// <summary>
        /// Gets or sets the place.
        /// </summary>
        public string Place { get; set; }

        /// <summary>
        /// Gets or sets the price.
        /// </summary>
        public string Price { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the phone.
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether allow commercial.
        /// </summary>
        public bool AllowCommercial { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the contact url.
        /// </summary>
        public string ContactUrl { get; set; }

        /// <summary>
        /// Gets or sets the param.
        /// </summary>
        public string Param { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the url.
        /// </summary>
        public Uri Url { get; set; }
    }
}