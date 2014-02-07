using LBCAlerterWeb.Models;
using LBCMapping;
using LBCMapping.Saver;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBCAlerterWeb
{
    public class EFSaver : ISaver
    {
        private static ApplicationDbContext db = new ApplicationDbContext();
        private Search m_search;

        public EFSaver(Search search)
        {
            m_search = search;
        }

        public bool Store(LBCMapping.Ad ad)
        {
            Models.Ad ads = db.Ads.FirstOrDefault(dbAd => dbAd.Search.ID == m_search.ID && dbAd.Url == ad.AdUrl);
            
            if(ads == null)
            {
                ads = new Models.Ad();
                ads.Search = m_search;
                ads.Url = ad.AdUrl;
                db.SaveChanges();
                return true;
            }
            else
                return false;
        }
    }
}
