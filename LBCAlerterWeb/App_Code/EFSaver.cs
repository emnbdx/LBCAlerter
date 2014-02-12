using LBCAlerterWeb.Models;
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
        private static ApplicationDbContext m_db;
        private Search m_search;

        public EFSaver(ApplicationDbContext db, Search search)
        {
            m_db = db;
            m_search = search;
        }

        public bool Store(LBCMapping.Ad ad)
        {
            Ad dbAd = m_db.Ads.FirstOrDefault(entity => entity.Search.ID == m_search.ID && entity.Url == ad.AdUrl);
            
            if(dbAd == null)
            {
                Ad tmpAd = Ad.ConvertLBCAd(ad);
                tmpAd.Search = m_search;
                m_db.Ads.Add(tmpAd);
                m_db.SaveChanges();
                return true;
            }
            else
                return false;
        }
    }
}
