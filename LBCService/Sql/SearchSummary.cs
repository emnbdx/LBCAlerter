using System;

namespace LBCService.Sql
{
    public class SearchSummary
    {
        public int Id { get; set; }

        public string Url { get; set; }

        public string KeyWord { get; set; }

        public int RefreshTime { get; set; }

        public string UserName { get; set; }

        public bool IsPremiumUser { get; set; }

        public bool Enabled { get; set; }

        public bool MailAlert { get; set; }

        public bool MailRecap { get; set; }

        public DateTime? LastRecap { get; set; }

        public int TodayAttempsCount { get; set; }

        public int AdsCount { get; set; }
    }
}