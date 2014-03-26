using LBCAlerterWeb.Models;
using LBCMapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBCService.Counter
{
    public class EFCounter : ICounter
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private int m_searchId;

        public EFCounter(int id)
        {
            m_searchId = id;
        }

        public void Count()
        {
            //Nothing to do
        }

        public void Result(int count)
        {
            Search search = db.Searches.FirstOrDefault(entry => entry.ID == m_searchId);

            search.Attempts.Add(
                new Attempt
                {
                    ProcessDate = DateTime.Now,
                    AdsFound = count,
                    Search = search
                });

            db.SaveChanges();
        }
    }
}
