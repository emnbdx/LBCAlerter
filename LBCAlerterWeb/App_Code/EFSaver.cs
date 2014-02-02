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
        private int m_searchId;

        public EFSaver(int searchId)
        {
            m_searchId = searchId;
        }

        public bool Store(Ad ad)
        {
            Search s = db.Searches.FirstOrDefault(search => search.SearchId == m_searchId);

            if (s.LastSearch == ad.AdUrl)
                return false;
            else
            {
                s.LastSearch = ad.AdUrl;
                db.SaveChanges();
                return true;
            }
        }
    }
}
