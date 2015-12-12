using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMToolBox.Mail
{
    public class Formater
    {
        private object m_source;
        private string m_unformatedString;
        private string m_formatedString;

        public Formater(object source, string unformated)
        {
            m_source = source;
            m_unformatedString = unformated;
        }

        public string GetFormated()
        {
            if(String.IsNullOrEmpty(m_formatedString))
            {
                if (m_source is Dictionary<string, object>)
                {
                    m_formatedString = m_unformatedString;
                    
                    Dictionary<string, object> tagDictionnary = (Dictionary<string, object>)m_source;
                    foreach (String key in tagDictionnary.Keys)
                    {
                        object tmp = tagDictionnary[key];

                        string result;
                        if (tmp is Int32)
                            result = Convert.ToInt32(tmp).ToString();
                        else if (tmp is Decimal)
                            result = Convert.ToDecimal(tmp).ToString();
                        else if (tmp is DateTime)
                            result = Convert.ToDateTime(tmp).ToString("dd/MM/yyyy HH:mm:ss");
                        else if (tmp is Boolean)
                            result = Convert.ToBoolean(tmp) ? "Oui" : "Non";
                        else
                            result = Convert.ToString(tmp);


                        m_formatedString = m_formatedString.Replace(key, result);
                    }
                }
                else
                    m_formatedString = m_source.ToString(m_unformatedString, new System.Globalization.CultureInfo("fr-fr"));
            }

            return m_formatedString;
        }
    }
}
