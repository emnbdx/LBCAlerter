using System;
using System.IO;
using System.Linq;

namespace LBCMapping.Saver
{
    /// <summary>
    /// Save ad in file ads+[keyword]
    /// </summary>
    public class FileSaver : ISaver
    {
        private string m_adFileName = "ads";

        public FileSaver(string fileSuffix)
        {
            m_adFileName += fileSuffix;

            if (!File.Exists(m_adFileName))
                File.Create(m_adFileName);
        }

        public bool Store(Ad ad)
        {
            bool urlFound = false;

            String[] lines = File.ReadAllLines(m_adFileName);
            if (lines.Contains(ad.AdUrl))
                urlFound = true;

            if (!urlFound)
            {
                File.AppendAllText(m_adFileName, ad.AdUrl + Environment.NewLine);
                return true;
            }
            else
                return false;
        }
    }
}