﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EMToolBox;
using HtmlAgilityPack;
using log4net;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Sockets;

namespace LBCMapping
{
    public class HtmlParser
    {
        private static ILog log = LogManager.GetLogger(typeof(HtmlParser));

        public const string URL_BASE = "http://www.leboncoin.fr/";
        private const string ENCODING = "iso-8859-15";
        private const string KEYWORD_URL_PARAM = "&q=";

        /// <summary>
        /// Replace relative link by absolute
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="xPathQuery"></param>
        private static void RelativeToAbsolute(HtmlDocument doc, string xPathQuery)
        {
            foreach (HtmlNode node in doc.DocumentNode.SelectNodes(xPathQuery))
            {
                if (node.Attributes.Contains("src") && node.Attributes["src"].Value.StartsWith("/"))
                    node.SetAttributeValue("src", URL_BASE + node.Attributes["src"].Value);
                if (node.Attributes.Contains("href") && node.Attributes["href"].Value.StartsWith("/"))
                    node.SetAttributeValue("href", URL_BASE + node.Attributes["href"].Value);
            }
        }

        private static void RemoveBackground(HtmlDocument doc, string xPathQuery)
        {
            if (doc.DocumentNode.SelectSingleNode(xPathQuery) != null)
            {
                if (doc.DocumentNode.SelectSingleNode(xPathQuery).Attributes["style"] != null)
                {
                    doc.DocumentNode.SelectSingleNode(xPathQuery).Attributes["style"].Value += "background-color: transparent;";
                }
                else
                {
                    doc.DocumentNode.SelectSingleNode(xPathQuery).Attributes.Add("style", "background-color: transparent;");
                }
            }
        }

        /// <summary>
        /// Do ajax call to get phone gif url by replacing javascript call
        /// </summary>
        /// <param name="phoneLink">Clikable link to display phone number</param>
        /// <returns>Url of phone number gif</returns>
        private static string GetPhoneUrl(String phoneLink)
        {
            //Get param
            if (String.IsNullOrEmpty(phoneLink))
                return null;
            int startIndex = phoneLink.IndexOf('(') + 1;
            string functionParam = phoneLink.Substring(startIndex, phoneLink.Length - startIndex - 1);
            string[] param = functionParam.Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries);

            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(param[0].Replace("\"", "").Trim() + "/ajapi/get/phone?list_id=" + param[1].Trim());

                string json;
                using (StreamReader sr = new StreamReader(httpWebRequest.GetResponse().GetResponseStream()))
                {
                    json = sr.ReadToEnd();
                }

                if (!String.IsNullOrEmpty(json) && json != "\"\"")
                {
                    JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                    return Convert.ToString(obj["phoneUrl"]);
                }
            }
            catch (Exception e)
            {
                log.Error("Erreur lors de la récupération du téléphone à partir de [" + phoneLink + "]", e);
            }

            return null;
        }

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
            HtmlNodeCollection dateNode = ad.SelectNodes("div[@class='date']/div");
            HtmlNode titleNode = ad.SelectSingleNode("div[@class='detail']/div[@class='title']");
            HtmlNode placementNode = ad.SelectSingleNode("div[@class='detail']/div[@class='placement']");
            HtmlNode priceNode = ad.SelectSingleNode("div[@class='detail']/div[@class='price']");
            HtmlNode imgNode = ad.SelectSingleNode("div[@class='image']/div[@class='image-and-nb']/img");

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
                if (dayMonth[1].StartsWith("ja"))
                    month = 1;
                else if (dayMonth[1].StartsWith("f"))
                    month = 2;
                else if (dayMonth[1].StartsWith("mar"))
                    month = 3;
                else if (dayMonth[1].StartsWith("av"))
                    month = 4;
                else if (dayMonth[1].StartsWith("mai"))
                    month = 5;
                else if (dayMonth[1].StartsWith("juin"))
                    month = 6;
                else if (dayMonth[1].StartsWith("juil"))
                    month = 7;
                else if (dayMonth[1].StartsWith("ao"))
                    month = 8;
                else if (dayMonth[1].StartsWith("s"))
                    month = 9;
                else if (dayMonth[1].StartsWith("o"))
                    month = 10;
                else if (dayMonth[1].StartsWith("n"))
                    month = 11;
                else if (dayMonth[1].StartsWith("d"))
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

            Ad tmp = new Ad()
            {
                Date = adDate,
                AdUrl = link.GetAttributeValue("href", ""),
                PictureUrl = imgNode != null ? imgNode.GetAttributeValue("src", "").Replace("thumbs", "images") : "",
                Place = placementNode != null ? placementNode.InnerText.Replace("\r", "").Replace("\n", "").Replace(" ", "") : "",
                Price = priceNode != null ? priceNode.InnerText.Trim() : "",
                Title = titleNode != null ? titleNode.InnerText.Trim() : ""
            };
            
            return tmp;
        }

        public static Ad ExtractAllAdInformation(Ad ad)
        {
            HtmlWeb web = new HtmlWeb();
            web.OverrideEncoding = Encoding.GetEncoding(ENCODING);
            HtmlDocument doc = web.Load(ad.AdUrl);
            HtmlNode adContent = doc.DocumentNode.SelectSingleNode("//div[@class='content-border']");

            List<string> pictures = new List<string>();
            if (adContent.SelectNodes("//div[@id='thumbs_carousel']//span[@class='thumbs']") != null)
                foreach (HtmlNode picture in adContent.SelectNodes("//div[@id='thumbs_carousel']//span[@class='thumbs']"))
                {
                    pictures.Add(picture.GetAttributeValue("style", "")
                        .Replace("background-image: url('", "")
                        .Replace("thumbs", "images")
                        .Replace("');", ""));
                }
            else if (adContent.SelectSingleNode("//div[@class='images_cadre']/a") != null)
            {
                HtmlNode picture = adContent.SelectSingleNode("//div[@class='images_cadre']/a");
                
                pictures.Add(picture.GetAttributeValue("style", "")
                        .Replace("background-image: url('", "")
                        .Replace("');", ""));
            }
            //HtmlNode phoneNode = adContent.SelectSingleNode("//span[@class='lbcPhone']/span[@id='phoneNumber']/a");
            //HtmlNode commercialNode = adContent.SelectSingleNode("//div[@class='lbc_links']/span[.='(Je refuse tout d&eacute;marchage commercial)']");
            HtmlNode nameNode = adContent.SelectSingleNode("//div[@class='upload_by']/a");
            HtmlNode emailNode = adContent.SelectSingleNode("//div[@class='lbc_links']/a[@class='sendMail']");
            List<string> parameters = new List<string>();
            if (adContent.SelectNodes("//div[contains(@class, 'lbcParamsContainer')]/div[contains(@class, 'lbcParams')]//tr") != null)
                foreach (HtmlNode parameter in adContent.SelectNodes("//div[contains(@class, 'lbcParamsContainer')]/div[contains(@class, 'lbcParams')]//tr"))
            {
                string title = parameter.SelectSingleNode("th").InnerText.Replace(":", "").Trim();

                string value = "";
                if (parameter.SelectSingleNode("td//span") != null)
                    value = parameter.SelectSingleNode("td//span").InnerText;
                else if (parameter.SelectSingleNode("td//a") != null)
                    value = parameter.SelectSingleNode("td//a").InnerText;
                else
                    value = parameter.SelectSingleNode("td").InnerText;

                parameters.Add(title + ": " + value);
            }
            HtmlNode descriptionNode = adContent.SelectSingleNode("//div[@class='AdviewContent']/div[@class='content']");

            ad.PictureUrl = String.Join(",", pictures);
            //TODO : find good solution to get phone number and commercial information
            //ad.Phone = phoneNode != null ? GetPhoneUrl(phoneNode.GetAttributeValue("href", "")) : "";
            //ad.AllowCommercial = commercialNode == null;
            ad.Name = nameNode != null ? nameNode.InnerText : "";
            ad.ContactUrl = emailNode != null ? emailNode.GetAttributeValue("href", "") : "";
            ad.Param = String.Join(",", parameters);
            ad.Description = descriptionNode != null ? descriptionNode.InnerHtml : "";

            return ad;
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
            if (doc.DocumentNode.SelectSingleNode("//div[@id='oas-top1']") != null)
                doc.DocumentNode.SelectSingleNode("//div[@id='oas-top1']").Remove();
            if (doc.DocumentNode.SelectSingleNode("//div[@id='cookieFrame']") != null)
                doc.DocumentNode.SelectSingleNode("//div[@id='cookieFrame']").Remove();

            RelativeToAbsolute(doc, "//script");
            RelativeToAbsolute(doc, "//link");
            
            RemoveBackground(doc, "//body[@id='all']");
            RemoveBackground(doc, "//div[@id='page_width']");
            RemoveBackground(doc, "//div[@class='search_box']");

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
            if (doc.DocumentNode.SelectSingleNode("//div[@id='cookieFrame']") != null)
                doc.DocumentNode.SelectSingleNode("//div[@id='cookieFrame']").Remove();

            RelativeToAbsolute(doc, "//script");
            RelativeToAbsolute(doc, "//link");

            RemoveBackground(doc, "//body[@id='all']");
            RemoveBackground(doc, "//div[@id='page_width']");
            RemoveBackground(doc, "//div[@class='search_box']");

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