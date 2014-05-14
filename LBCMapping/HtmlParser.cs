using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EMToolBox;
using HtmlAgilityPack;
using log4net;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;

namespace LBCMapping
{
    public class HtmlParser
    {
        private static ILog log = LogManager.GetLogger(typeof(HtmlParser));

        public const string URL_BASE = "http://www.leboncoin.fr/";
        public const string ENCODING = "iso-8859-15";
        public const string KEYWORD_URL_PARAM = "&q=";

        /// <summary>
        /// Due to encoding issue not solvable, replace some parameter in criteria
        /// </summary>
        /// <param name="basePath">Base path for search</param>
        /// <returns>Cleaned path</returns>
        public static string CleanCriteria(string basePath)
        {
            return basePath
                //Remove bad stuff
                .Replace("f=a&th=1", "o={0}")
                //Clean category
                .Replace("toutes_cat_gories", "annonces")
                .Replace("ventes_immobili_res", "ventes_immobilieres")
                .Replace("consoles_jeux_vid_o", "consoles_jeux_video")
                .Replace("t_l_phonie", "telephonie")
                .Replace("electrom_nager", "electromenager")
                .Replace("d_coration", "decoration")
                .Replace("v_tements", "vetements")
                .Replace("equipement_b_b_", "equipement_bebe")
                .Replace("vetements_b_b_", "vetements_bebe")
                .Replace("v_los", "velos")
                .Replace("mat_riel_agricole", "materiel_agricole")
                .Replace("outillage_mat_riaux_2nd_oeuvre", "outillage_materiaux_2nd_oeuvre")
                .Replace("_quipements_industriels", "equipements_industriels")
                .Replace("restauration_h_tellerie", "restauration_hotellerie")
                .Replace("commerces_march_s", "commerces_marches")
                .Replace("mat_riel_m_dical", "materiel_medical")
                .Replace("ev_nements", "evenements")
                //Clean location
                .Replace("pyr_n_es_atlantiques", "pyrenees_atlantiques")
                .Replace("puy_de_d_me", "puy_de_dome")
                .Replace("c_te_d_or", "cote_d_or")
                .Replace("ni_vre", "nievre")
                .Replace("sa_ne_et_loire", "saone_et_loire")
                .Replace("c_tes_d_armor", "cotes_d_armor")
                .Replace("finist_re", "finistere")
                .Replace("haute_sa_ne", "haute_saone")
                .Replace("h_rault", "herault")
                .Replace("loz_re", "lozere")
                .Replace("corr_ze", "correze")
                .Replace("ari_ge", "ariege")
                .Replace("hautes_pyr_n_es", "hautes_pyrenees")
                .Replace("vend_e", "vendee")
                .Replace("deux_s_vres", "deux_sevres")
                .Replace("bouches_du_rh_ne", "bouches_du_rhone")
                .Replace("ard_che", "ardeche")
                .Replace("dr_me", "drome")
                .Replace("isère", "isere")
                .Replace("rh_ne", "rhone");
        }

        /// <summary>
        /// Extract keyword &q= param from url
        /// </summary>
        /// <returns>Keyword extracted and well formated</returns>
        public static string ExtractKeyWordFromCriteria(string criteria)
        {
            string keyword;

            //Cut path to get keyword
            int startIndex = criteria.IndexOf(HtmlParser.KEYWORD_URL_PARAM);
            int endIndex = criteria.IndexOf("&", startIndex + HtmlParser.KEYWORD_URL_PARAM.Length);
            if (endIndex == -1)
            {
                keyword = criteria.Substring(startIndex + HtmlParser.KEYWORD_URL_PARAM.Length, criteria.Length - (startIndex + HtmlParser.KEYWORD_URL_PARAM.Length));
            }
            else
            {
                keyword = criteria.Substring(startIndex + HtmlParser.KEYWORD_URL_PARAM.Length, endIndex - (startIndex + HtmlParser.KEYWORD_URL_PARAM.Length));
            }
            keyword = keyword.Replace("+", " ");

            return keyword;
        }

        /// <summary>
        /// Parse html div and return Ad instance
        /// </summary>
        /// <param name="link">Base node for parsing</param>
        /// <returns>Ad instance with all data collected</returns>
        public static Ad ExtractAdInformation(HtmlNode link)
        {
            HtmlNode ad = link.SelectSingleNode("div[@class='lbc']");

            //Get all value
            HtmlNodeCollection dateNode = ad.SelectNodes("div[@class='date']//div");
            HtmlNode titleNode = ad.SelectSingleNode("div[@class='detail']//div[@class='title']");
            HtmlNode placementNode = ad.SelectSingleNode("div[@class='detail']//div[@class='placement']");
            HtmlNode priceNode = ad.SelectSingleNode("div[@class='detail']//div[@class='price']");
            HtmlNode imgNode = ad.SelectSingleNode("div[@class='image']//div[@class='image-and-nb']//img");

            //Make good date
            int month, day, hour, minute;
            string date = dateNode != null ? dateNode[0].InnerText.Trim() : "";
            if (date == "Aujourd'hui")
            {
                day = DateTime.Now.Day;
                month = DateTime.Now.Month;
            }
            else if (date == "Hier")
            {
                DateTime yesterday = DateTime.Now.AddDays(-1);
                day = yesterday.Day;
                month = yesterday.Month;
            }
            else
            {
                string[] dayMonth = date.Split(' ');
                day = Convert.ToInt32(dayMonth[0]);
                if (dayMonth[1] == "jan")
                    month = 1;
                else if (dayMonth[1] == "fev")
                    month = 2;
                else if (dayMonth[1] == "mars")
                    month = 3;
                else if (dayMonth[1] == "avr")
                    month = 4;
                else if (dayMonth[1] == "mai")
                    month = 5;
                else if (dayMonth[1] == "jui")
                    month = 6;
                else if (dayMonth[1] == "juil")
                    month = 7;
                else if (dayMonth[1] == "aout")
                    month = 8;
                else if (dayMonth[1] == "sep")
                    month = 9;
                else if (dayMonth[1] == "oct")
                    month = 10;
                else if (dayMonth[1] == "nov")
                    month = 11;
                else if (dayMonth[1] == "déc")
                    month = 12;
                else
                    month = DateTime.Now.Month;
            }
            string time = dateNode != null ? dateNode[1].InnerText.Trim() : "";
            string[] hourMinute = time.Split(':');
            hour = Convert.ToInt32(hourMinute[0]);
            minute = Convert.ToInt32(hourMinute[1]);

            DateTime adDate = new DateTime(DateTime.Now.Year, month, day, hour, minute, 0);
            if (adDate > DateTime.Now)
                adDate = adDate.AddYears(-1);

            /*HtmlWeb web = new HtmlWeb();
            web.OverrideEncoding = Encoding.GetEncoding(ENCODING);
            HtmlDocument doc = web.Load(link.GetAttributeValue("href", ""));

            HtmlNode adPage = doc.DocumentNode;

            HtmlNode emailNode = adPage.SelectSingleNode("div[@class='lbc_links']//a");
            HtmlNode phoneNode = adPage.SelectSingleNode("span[@class='lbcPhone']//span[@class='phoneNumber']//a");*/

            return new Ad(adDate,
                            link.GetAttributeValue("href", ""),
                            imgNode != null ? imgNode.GetAttributeValue("src", "") : "",
                            placementNode != null ? placementNode.InnerText.Replace("\r", "").Replace("\n", "").Replace(" ", "") : "",
                            priceNode != null ? priceNode.InnerText.Trim() : "",
                            titleNode != null ? titleNode.InnerText.Trim() : "");
        }

        /// <summary>
        /// Do ajax call to get phone gif url by replacing javascript call
        /// </summary>
        /// <param name="phoneLink">Clikable link to display phone number</param>
        /// <returns>Url of phone number gif</returns>
        private static string GetPhoneUrl(String phoneLink)
        {
            //Get param
            int startIndex = phoneLink.IndexOf('(');
            string functionParam = phoneLink.Substring(startIndex, phoneLink.Length - startIndex - 1);
            string[] param = functionParam.Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries);

            /*
             * Original JSON
             * 
                jQuery.ajax({
	                type:"GET",
	                async:true,
	                crossDomain:true,
	                url:"http://www2.leboncoin.fr/ajapi/get/phone",
	                data:{
		                list_id:640715994
	                },
	                format:"jsonp"
                }).done(function(f){
	                alert(f.phoneUrl);
                })
             * 
             */

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(param[0] + "/ajapi/get/phone");
            httpWebRequest.ContentType = "text/json";
            httpWebRequest.Method = "GET";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = "{ " +  
                              "     data: { " +
		                      "         list_id: " + param[1] +
                              "     } " +
	                          " }";

                streamWriter.Write(json);
            }
            
            JObject result;
            using (var streamReader = new StreamReader(httpWebRequest.GetResponse().GetResponseStream()))
            {
                result = JObject.Parse(streamReader.ReadToEnd());
            }
            return "";
        }

        /// <summary>
        /// Return clean home page only map and region name
        /// </summary>
        /// <returns>Html code of page</returns>
        public static string GetHomePage()
        {
            HtmlWeb web = new HtmlWeb();
            web.OverrideEncoding = Encoding.GetEncoding(ENCODING);
            HtmlDocument doc = web.Load(URL_BASE);

            //Delete unused div
            if (doc.DocumentNode.SelectSingleNode("//table[@id='TableContentTop']") != null)
                doc.DocumentNode.SelectSingleNode("//table[@id='TableContentTop']").Remove();
            if (doc.DocumentNode.SelectSingleNode("//span[@class='SeparatorText']//..") != null)
                doc.DocumentNode.SelectSingleNode("//span[@class='SeparatorText']//..").Remove();
            if (doc.DocumentNode.SelectSingleNode("//div[@class='Deposer']") != null)
                doc.DocumentNode.SelectSingleNode("//div[@class='Deposer']").Remove();
            if (doc.DocumentNode.SelectSingleNode("//div[@id='incr_renc_home_button']") != null)
                doc.DocumentNode.SelectSingleNode("//div[@id='incr_renc_home_button']").Remove();
            if (doc.DocumentNode.SelectSingleNode("//div[@id='Footer']") != null)
                doc.DocumentNode.SelectSingleNode("//div[@id='Footer']").Remove();
            if (doc.DocumentNode.SelectSingleNode("//div[@id='Banner_sky']") != null)
                doc.DocumentNode.SelectSingleNode("//div[@id='Banner_sky']").Remove();

            //Replace relative link by absolute
            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//script"))
            {
                if (node.Attributes.Contains("src") && node.Attributes["src"].Value.StartsWith("/"))
                    node.SetAttributeValue("src", URL_BASE + node.Attributes["src"].Value);
            }

            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//link"))
            {
                if (node.Attributes.Contains("href") && node.Attributes["href"].Value.StartsWith("/"))
                    node.SetAttributeValue("href", URL_BASE + node.Attributes["href"].Value);
            }

            return doc.DocumentNode.WriteTo();
        }

        /// <summary>
        /// Return clean search page only criteria section
        /// </summary>
        /// <param name="path">Cirteria path from home page (contain region)</param>
        /// <returns>Html code of page</returns>
        public static string GetCriteriaPage(string path)
        {
            string requestUrl;
            if(path.StartsWith(URL_BASE))
                requestUrl = String.Format(CleanCriteria(path), 0);
            else
                requestUrl = String.Format(URL_BASE + CleanCriteria(path), 0);

            HtmlWeb web = new HtmlWeb();
            web.OverrideEncoding = Encoding.GetEncoding(ENCODING);
            HtmlDocument doc = web.Load(requestUrl);

            //Delete unused div
            doc.DocumentNode.SelectSingleNode("//div[@id='account_login_f']").Remove();
            doc.DocumentNode.SelectSingleNode("//header[@id='headermain']").Remove();
            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//nav"))
                node.Remove();
            if (doc.DocumentNode.SelectSingleNode("//div[@class='oas-x01']") != null)
                doc.DocumentNode.SelectSingleNode("//div[@class='oas-x01']").Remove();
            if (doc.DocumentNode.SelectSingleNode("//div[@class='oas-x02']") != null)
                doc.DocumentNode.SelectSingleNode("//div[@class='oas-x02']").Remove();
            if (doc.DocumentNode.SelectSingleNode("//div[@class='oas-x03']") != null)
                doc.DocumentNode.SelectSingleNode("//div[@class='oas-x03']").Remove();
            if (doc.DocumentNode.SelectSingleNode("//div[@class='oas-x04']") != null)
                doc.DocumentNode.SelectSingleNode("//div[@class='oas-x04']").Remove();
            if (doc.DocumentNode.SelectSingleNode("//div[@class='oas-top']") != null)
                doc.DocumentNode.SelectSingleNode("//div[@class='oas-top']").Remove();
            if (doc.DocumentNode.SelectSingleNode("//div[@id='account_submenu']") != null)
                doc.DocumentNode.SelectSingleNode("//div[@id='account_submenu']").Remove();
            if (doc.DocumentNode.SelectSingleNode("//div[@class='comment']") != null)
                doc.DocumentNode.SelectSingleNode("//div[@class='comment']").Remove();
            if (doc.DocumentNode.SelectSingleNode("//div[@class='content-border list']") != null)
                doc.DocumentNode.SelectSingleNode("//div[@class='content-border list']").Remove();
            if (doc.DocumentNode.SelectSingleNode("//div[@id='categories_container']") != null)
                doc.DocumentNode.SelectSingleNode("//div[@id='categories_container']").Remove();

            //Replace relative link by absolute
            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//script"))
            {
                if (node.Attributes.Contains("src") && node.Attributes["src"].Value.StartsWith("/"))
                    node.SetAttributeValue("src", URL_BASE + node.Attributes["src"].Value);
            }

            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//link"))
            {
                if (node.Attributes.Contains("href") && node.Attributes["href"].Value.StartsWith("/"))
                    node.SetAttributeValue("href", URL_BASE + node.Attributes["href"].Value);
            }

            return doc.DocumentNode.WriteTo();
        }

        /// <summary>
        /// Return node list corresponding to ads, from criteria and page
        /// </summary>
        /// <param name="criteria">Criteria for search</param>
        /// <param name="page">page to search</param>
        /// <returns>Node list, you can browse each with GetAdInformation to convert html to object</returns>
        public static List<HtmlNode> GetHtmlAd(string criteria, int page)
        {
            string requestUrl = String.Format(URL_BASE + CleanCriteria(criteria), page);

            log.Debug("Récupération des annonces à l'url [" + requestUrl + "]");

            HtmlWeb web = new HtmlWeb();
            web.OverrideEncoding = Encoding.GetEncoding(ENCODING);
            HtmlDocument doc = web.Load(requestUrl);

            HtmlNodeCollection links = doc.DocumentNode.SelectNodes("//div[@class='list-lbc']//a");
            
            if (links == null)
                return null;
            else
                return links.ToList();
        }
    }
}