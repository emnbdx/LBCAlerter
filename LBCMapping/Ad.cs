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

        private string m_emailUrl;

        private string m_phoneUrl;

        private Uri m_url;

        public Ad(DateTime date, string adUrl, string pictureUrl, string place, string price, string title, string emailUrl, string phoneUrl)
        {
            m_date = date;
            m_adUrl = adUrl;
            m_pictureUrl = pictureUrl;
            m_place = place;
            m_price = price;
            m_title = title;
            m_emailUrl = emailUrl;
            m_phoneUrl = phoneUrl;
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

        public string EmailUrl
        {
            get { return m_emailUrl; }
            set { m_emailUrl = value; }
        }

        public string PhoneUrl
        {
            get { return m_phoneUrl; }
            set { m_phoneUrl = value; }
        }

        public Uri Url
        {
            get { return m_url; }
            set { m_url = value; }
        }
    }
}