using System;

namespace LBCMapping
{
    public class Ad
    {
        public DateTime Date { get; set; }

        public string AdUrl { get; set; }

        public string PictureUrl { get; set; }

        public string Place { get; set; }

        public string Price { get; set; }

        public string Title { get; set; }

        public string Phone { get; set; }

        public bool AllowCommercial { get; set; }

        public string Name { get; set; }

        public string ContactUrl { get; set; }

        public string Param { get; set; }

        public string Description { get; set; }

        public Uri Url { get; set; }
    }
}