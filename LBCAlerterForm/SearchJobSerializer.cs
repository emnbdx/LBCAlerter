using LBCMapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LBCAlerter
{
    public class SearchJobSerializer
    {
        private static string m_fileName = "criterion.xml";

        public static List<SearchJob> Load()
        {
            if (!File.Exists(m_fileName))
                File.Create(m_fileName);

            var serializer = new XmlSerializer(typeof(List<SearchJob>));
            using (StreamReader sr = new StreamReader(m_fileName))
                return (List<SearchJob>)serializer.Deserialize(sr);
        }

        public static void Save(List<SearchJob> search)
        {
            if (!File.Exists(m_fileName))
                File.Create(m_fileName);

            var serializer = new XmlSerializer(typeof(List<SearchJob>));
            using (StreamWriter sw = new StreamWriter(m_fileName))
                serializer.Serialize(sw, search);
        }
    }
}
