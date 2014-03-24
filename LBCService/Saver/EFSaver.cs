﻿using LBCAlerterWeb.Models;
using LBCMapping;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBCService.Saver
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
                    Search s = db.Searches.FirstOrDefault(entry => entry.ID == m_searchId);
                    if(s == null)
                        throw new Exception("Recherche inexistante...");
                    tmpAd.Search = s;
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