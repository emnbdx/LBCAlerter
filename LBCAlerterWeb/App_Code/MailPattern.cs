using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace LBCAlerterWeb.App_Code
{
    public class MailPattern
    {
        public static string GetPattern(MailType type)
        {
            String patternBody = "";

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "EMToolBox.Mail.";

            switch(type)
            {
                case MailType.Ad:
                    resourceName += "lbc-ad.html"; break;
                case MailType.Confirmation:
                    resourceName += "lbc-confirmation.html"; break;
                case MailType.Recap:
                    resourceName += "lbc-recap.html"; break;
                case MailType.RecapAd:
                    resourceName += "lbc-recap-ad.html"; break;
                default: throw new Exception("unknow pattern");

            }

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader sr = new StreamReader(stream))
            {
                patternBody = sr.ReadToEnd();
            }

            return patternBody;
        }
    }

    public enum MailType
    {
        Ad,
        Confirmation,
        Recap,
        RecapAd
    }
}