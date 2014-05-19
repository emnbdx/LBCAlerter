using System;

namespace LBCMapping
{
    public class Ad
    {
        private DateTime m_date;

        private string m_adUrl;

        private string m_pictureUrl;

        private string m_place;

        private string m_price;

        private string m_title;

        private string m_phone;

        private bool m_allowCommercial;

        private string m_name;

        private string m_contactUrl;

        private string m_param;

        private string m_description;

        private Uri m_url;

        public Ad(DateTime date, string adUrl, string pictureUrl, string place, string price, string title)
        {
            m_date = date;
            m_adUrl = adUrl;
            m_pictureUrl = pictureUrl;
            m_place = place;
            m_price = price;
            m_title = title;
        }

        public Ad(Uri url)
        {
            m_url = url;
        }

        public Ad(string url)
        {
            m_url = new Uri(url);
        }

        public DateTime Date
        {
            get { return m_date; }
            set { m_date = value; }
        }

        public string AdUrl
        {
            get { return m_adUrl; }
            set { m_adUrl = value; }
        }

        public string PictureUrl
        {
            get { return m_pictureUrl; }
            set { m_pictureUrl = value; }
        }

        public string Place
        {
            get { return m_place; }
            set { m_place = value; }
        }

        public string Price
        {
            get { return m_price; }
            set { m_price = value; }
        }

        public string Title
        {
            get { return m_title; }
            set { m_title = value; }
        }

        public string Phone
        {
            get { return m_phone; }
            set { m_phone = value; }
        }

        public bool AllowCommercial
        {
            get { return m_allowCommercial; }
            set { m_allowCommercial = value; }
        }

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        public string ContactUrl
        {
            get { return m_contactUrl; }
            set { m_contactUrl = value; }
        }

        public string Param
        {
            get { return m_param; }
            set { m_param = value; }
        }

        public string Description
        {
            get { return m_description; }
            set { m_description = value; }
        }

        public Uri Url
        {
            get { return m_url; }
            set { m_url = value; }
        }
    }
}