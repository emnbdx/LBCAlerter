using LBCAlerter.Properties;
using LBCMapping;
using LBCMapping.Saver;

namespace LBCAlerter
{
    public class SettingsSaver : ISaver
    {
        private string m_keyword;

        public SettingsSaver(string keyword)
        {
            if (Settings.Default.OldAds == null)
                Settings.Default.OldAds = new System.Collections.Specialized.StringCollection();

            m_keyword = keyword;
        }

        public bool Store(Ad ad)
        {
            bool urlFound = false;

            if (Settings.Default.OldAds.Contains(m_keyword + ";" + ad.AdUrl))
                urlFound = true;

            if (!urlFound)
            {
                Settings.Default.OldAds.Add(m_keyword + ";" + ad.AdUrl);
                Settings.Default.Save();
                return true;
            }
            else
                return false;
        }
    }
}