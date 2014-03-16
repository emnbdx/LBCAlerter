using LBCAlerterWeb.Models;
using LBCMapping;
using LBCMapping.Saver;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBCService
{
    public class EFSaver : ISaver
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private int m_searchId;

        public EFSaver(int id)
        {
            m_searchId = id;
        }

        public bool Store(LBCMapping.Ad ad)
        {
            LBCAlerterWeb.Models.Ad dbAd = db.Ads.FirstOrDefault(entry => entry.Search.ID == m_searchId && entry.Url == ad.AdUrl);

            if (dbAd == null || dbAd.Date != ad.Date)
            {
                if (dbAd == null)
                {
                    LBCAlerterWeb.Models.Ad tmpAd = LBCAlerterWeb.Models.Ad.ConvertLBCAd(ad);
                    tmpAd.Search = db.Searches.FirstOrDefault(entry => entry.ID == m_searchId);
                    db.Ads.Add(tmpAd);
                }
                else
                    dbAd.Date = ad.Date;

                db.SaveChanges();
                return true;
            }
            else
                return false;
        }
    }
}
