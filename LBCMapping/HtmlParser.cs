namespace LBCMapping
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    
    using HtmlAgilityPack;
    using log4net;

    /// <summary>
    /// The html parser.
    /// </summary>
    public class HtmlParser
    {
        /// <summary>
        /// The ur l_ base.
        /// </summary>
        public const string UrlBase = "http://www.leboncoin.fr/";

        /// <summary>
        /// The encoding.
        /// </summary>
        private const string Encoding = "iso-8859-15";

        /// <summary>
        /// The log.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(HtmlParser));

        /// <summary>
        /// Due to encoding issue not solvable, replace some parameter in criteria
        /// </summary>
        /// <param name="basePath">Base path for search</param>
        /// <returns>Cleaned path</returns>
        public static string CleanCriteria(string basePath)
        {
            return basePath

                // Remove bad stuff
                .Replace("f=a&th=1", "o={0}")

                // Clean category
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

                // Clean location
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
                .Replace("is_re", "isere")
                .Replace("rh_ne", "rhone")
                .Replace("franche_comt_", "franche_comte")
                .Replace("midi_pyr_n_es", "midi_pyrenees")
                .Replace("provence_alpes_c_te_d_azur", "provence_alpes_cote_d_azur")
                .Replace("r_union", "reunion");
        }

        /// <summary>
        /// Extract keyword param from url
        /// </summary>
        /// <param name="criteria">
        /// The criteria.
        /// </param>
        /// <returns>
        /// The <see cref="string"/> keyword extracted and well formated.
        /// </returns>
        public static string ExtractKeyWordFromCriteria(string criteria)
        {
            // Get only param
            var tmpCriteria = criteria.Substring(criteria.LastIndexOf('/'), criteria.Length - criteria.LastIndexOf('/'));
            var tmp = HttpUtility.ParseQueryString(tmpCriteria);

            var location = string.Empty;
            if (tmp.AllKeys.Contains("location"))
            {
                location = tmp["location"];
            }

            var keyword = tmp.AllKeys.Contains("q") ? tmp["q"] : criteria.Substring(0, criteria.LastIndexOf('/'));
            keyword = keyword.Replace("+", " ").Replace("/", " ").Replace("_", " ");

            return keyword + " " + location;
        }

        /// <summary>
        /// Parse html div and return Ad instance
        /// </summary>
        /// <param name="link">Base node for parsing</param>
        /// <returns>Ad instance with all data collected</returns>
        public static Ad ExtractAdInformation(HtmlNode link)
        {
            var ad = link.SelectSingleNode("div[@class='lbc']");

            // Get all value
            var dateNode = ad.SelectNodes("div[@class='date']/div");
            var titleNode = ad.SelectSingleNode("div[@class='detail']/div[@class='title']");
            var placementNode = ad.SelectSingleNode("div[@class='detail']/div[@class='placement']");
            var priceNode = ad.SelectSingleNode("div[@class='detail']/div[@class='price']");
            var imgNode = ad.SelectSingleNode("div[@class='image']/div[@class='image-and-nb']/img");

            // Make good date
            int month, day;
            var date = dateNode != null ? dateNode[0].InnerText.Trim() : string.Empty;
            switch (date)
            {
                case "Aujourd'hui":
                    day = DateTime.Now.Day;
                    month = DateTime.Now.Month;
                    break;
                case "Hier":
                    var yesterday = DateTime.Now.AddDays(-1);
                    day = yesterday.Day;
                    month = yesterday.Month;
                    break;
                default:
                    var dayMonth = date.Split(' ');
                    day = Convert.ToInt32(dayMonth[0]);
                    if (dayMonth[1].StartsWith("ja"))
                    {
                        month = 1;
                    }
                    else if (dayMonth[1].StartsWith("f"))
                    {
                        month = 2;
                    }
                    else if (dayMonth[1].StartsWith("mar"))
                    {
                        month = 3;
                    }
                    else if (dayMonth[1].StartsWith("av"))
                    {
                        month = 4;
                    }
                    else if (dayMonth[1].StartsWith("mai"))
                    {
                        month = 5;
                    }
                    else if (dayMonth[1].StartsWith("juin"))
                    {
                        month = 6;
                    }
                    else if (dayMonth[1].StartsWith("juil"))
                    {
                        month = 7;
                    }
                    else if (dayMonth[1].StartsWith("ao"))
                    {
                        month = 8;
                    }
                    else if (dayMonth[1].StartsWith("s"))
                    {
                        month = 9;
                    }
                    else if (dayMonth[1].StartsWith("o"))
                    {
                        month = 10;
                    }
                    else if (dayMonth[1].StartsWith("n"))
                    {
                        month = 11;
                    }
                    else if (dayMonth[1].StartsWith("d"))
                    {
                        month = 12;
                    }
                    else
                    {
                        month = DateTime.Now.Month;
                    }

                    break;
            }

            var time = dateNode != null ? dateNode[1].InnerText.Trim() : string.Empty;
            var hourMinute = time.Split(':');
            var hour = Convert.ToInt32(hourMinute[0]);
            var minute = Convert.ToInt32(hourMinute[1]);

            var realDate = new DateTime(DateTime.Now.Year, month, day, hour, minute, 0);
            if (realDate > DateTime.Now)
            {
                realDate = realDate.AddYears(-1);
            }

            var tmp = new Ad
                          {
                              Date = realDate,
                              AdUrl = link.GetAttributeValue("href", string.Empty),
                              PictureUrl =
                                  imgNode != null
                                      ? imgNode.GetAttributeValue("src", string.Empty).Replace("thumbs", "images")
                                      : string.Empty,
                              Place =
                                  placementNode != null
                                      ? placementNode.InnerText.Replace("\r", string.Empty)
                                            .Replace("\n", string.Empty)
                                            .Replace(" ", string.Empty)
                                      : string.Empty,
                              Price = priceNode != null ? priceNode.InnerText.Trim() : string.Empty,
                              Title = titleNode != null ? titleNode.InnerText.Trim() : string.Empty
                          };

            return tmp;
        }

        /// <summary>
        /// The extract all ad information.
        /// </summary>
        /// <param name="ad">
        /// The ad.
        /// </param>
        /// <returns>
        /// The <see cref="Ad"/>.
        /// </returns>
        public static Ad ExtractAllAdInformation(Ad ad)
        {
            var web = new HtmlWeb { OverrideEncoding = System.Text.Encoding.GetEncoding(Encoding) };
            var doc = web.Load(ad.AdUrl);
            var content = doc.DocumentNode.SelectSingleNode("//div[@class='content-border']");

            var pictures = new List<string>();
            if (content.SelectNodes("//div[@id='thumbs_carousel']//span[@class='thumbs']") != null)
            {
                foreach (var picture in content.SelectNodes("//div[@id='thumbs_carousel']//span[@class='thumbs']"))
                {
                    pictures.Add(
                        picture.GetAttributeValue("style", string.Empty)
                            .Replace("background-image: url('", string.Empty)
                            .Replace("thumbs", "images")
                            .Replace("');", string.Empty));
                }
            }
            else if (content.SelectSingleNode("//div[@class='images_cadre']/a") != null)
            {
                var picture = content.SelectSingleNode("//div[@class='images_cadre']/a");

                pictures.Add(
                    picture.GetAttributeValue("style", string.Empty)
                        .Replace("background-image: url('", string.Empty)
                        .Replace("');", string.Empty));
            }

            /*HtmlNode phoneNode = adContent.SelectSingleNode("//span[@class='lbcPhone']/span[@id='phoneNumber']/a");
            HtmlNode commercialNode = adContent.SelectSingleNode("//div[@class='lbc_links']/span[.='(Je refuse tout d&eacute;marchage commercial)']");*/
            var nameNode = content.SelectSingleNode("//div[@class='upload_by']/a");
            var emailNode = content.SelectSingleNode("//div[@class='lbc_links']/a[@class='sendMail']");
            var parameters = new List<string>();
            if (
                content.SelectNodes(
                    "//div[contains(@class, 'lbcParamsContainer')]/div[contains(@class, 'lbcParams')]//tr") != null)
            {
                foreach (var parameter in
                    content.SelectNodes(
                            "//div[contains(@class, 'lbcParamsContainer')]/div[contains(@class, 'lbcParams')]//tr"))
                {
                    var title = parameter.SelectSingleNode("th").InnerText.Replace(":", string.Empty).Trim();
                    string value;

                    if (parameter.SelectSingleNode("td//span") != null)
                    {
                        value = parameter.SelectSingleNode("td//span").InnerText;
                    }
                    else if (parameter.SelectSingleNode("td//a") != null)
                    {
                        value = parameter.SelectSingleNode("td//a").InnerText;
                    }
                    else
                    {
                        value = parameter.SelectSingleNode("td").InnerText;
                    }

                    parameters.Add(title + ": " + value);
                }
            }

            var descriptionNode = content.SelectSingleNode("//div[@class='AdviewContent']/div[@class='content']");

            ad.PictureUrl = string.Join(",", pictures);
            /*TODO : find good solution to get phone number and commercial information
            ad.Phone = phoneNode != null ? GetPhoneUrl(phoneNode.GetAttributeValue("href", "")) : "";
            ad.AllowCommercial = commercialNode == null;*/
            ad.Name = nameNode != null ? nameNode.InnerText : string.Empty;
            ad.ContactUrl = emailNode != null ? emailNode.GetAttributeValue("href", string.Empty) : string.Empty;
            ad.Param = string.Join(",", parameters);
            ad.Description = descriptionNode != null ? descriptionNode.InnerHtml : string.Empty;

            return ad;
        }        

        /// <summary>
        /// Return clean home page only map and region name
        /// </summary>
        /// <returns>Html code of page</returns>
        public static string GetHomePage()
        {
            var web = new HtmlWeb { OverrideEncoding = System.Text.Encoding.GetEncoding(Encoding) };
            var doc = web.Load(UrlBase);

            // Delete unused div
            if (doc.DocumentNode.SelectSingleNode("//table[@id='TableContentTop']") != null)
            {
                doc.DocumentNode.SelectSingleNode("//table[@id='TableContentTop']").Remove();
            }

            if (doc.DocumentNode.SelectSingleNode("//span[@class='SeparatorText']//..") != null)
            {
                doc.DocumentNode.SelectSingleNode("//span[@class='SeparatorText']//..").Remove();
            }

            if (doc.DocumentNode.SelectSingleNode("//div[@class='Deposer']") != null)
            {
                doc.DocumentNode.SelectSingleNode("//div[@class='Deposer']").Remove();
            }

            if (doc.DocumentNode.SelectSingleNode("//div[@id='incr_renc_home_button']") != null)
            {
                doc.DocumentNode.SelectSingleNode("//div[@id='incr_renc_home_button']").Remove();
            }

            if (doc.DocumentNode.SelectSingleNode("//div[@id='Footer']") != null)
            {
                doc.DocumentNode.SelectSingleNode("//div[@id='Footer']").Remove();
            }

            if (doc.DocumentNode.SelectSingleNode("//div[@id='Banner_sky']") != null)
            {
                doc.DocumentNode.SelectSingleNode("//div[@id='Banner_sky']").Remove();
            }

            if (doc.DocumentNode.SelectSingleNode("//div[@id='oas-top1']") != null)
            {
                doc.DocumentNode.SelectSingleNode("//div[@id='oas-top1']").Remove();
            }

            if (doc.DocumentNode.SelectSingleNode("//div[@id='cookieFrame']") != null)
            {
                doc.DocumentNode.SelectSingleNode("//div[@id='cookieFrame']").Remove();
            }

            RelativeToAbsolute(doc, "//script");
            RelativeToAbsolute(doc, "//link");

            return doc.DocumentNode.WriteTo();
        }

        /// <summary>
        /// Return clean search page only criteria section
        /// </summary>
        /// <param name="path">Cirteria path from home page (contain region)</param>
        /// <returns>Html code of page</returns>
        public static string GetCriteriaPage(string path)
        {
            var requestUrl = path.StartsWith(UrlBase) ? string.Format(CleanCriteria(path), 0) : string.Format(UrlBase + CleanCriteria(path), 0);

            var web = new HtmlWeb { OverrideEncoding = System.Text.Encoding.GetEncoding(Encoding) };
            var doc = web.Load(requestUrl);

            // Delete unused div
            doc.DocumentNode.SelectSingleNode("//div[@id='account_login_f']").Remove();
            doc.DocumentNode.SelectSingleNode("//header[@id='headermain']").Remove();
            foreach (var node in doc.DocumentNode.SelectNodes("//nav"))
            {
                node.Remove();
            }

            if (doc.DocumentNode.SelectSingleNode("//div[@class='oas-x01']") != null)
            {
                doc.DocumentNode.SelectSingleNode("//div[@class='oas-x01']").Remove();
            }

            if (doc.DocumentNode.SelectSingleNode("//div[@class='oas-x02']") != null)
            {
                doc.DocumentNode.SelectSingleNode("//div[@class='oas-x02']").Remove();
            }

            if (doc.DocumentNode.SelectSingleNode("//div[@class='oas-x03']") != null)
            {
                doc.DocumentNode.SelectSingleNode("//div[@class='oas-x03']").Remove();
            }

            if (doc.DocumentNode.SelectSingleNode("//div[@class='oas-x04']") != null)
            {
                doc.DocumentNode.SelectSingleNode("//div[@class='oas-x04']").Remove();
            }

            if (doc.DocumentNode.SelectSingleNode("//div[@class='oas-top']") != null)
            {
                doc.DocumentNode.SelectSingleNode("//div[@class='oas-top']").Remove();
            }

            if (doc.DocumentNode.SelectSingleNode("//div[@id='account_submenu']") != null)
            {
                doc.DocumentNode.SelectSingleNode("//div[@id='account_submenu']").Remove();
            }

            if (doc.DocumentNode.SelectSingleNode("//div[@class='comment']") != null)
            {
                doc.DocumentNode.SelectSingleNode("//div[@class='comment']").Remove();
            }

            if (doc.DocumentNode.SelectSingleNode("//div[@class='content-border list']") != null)
            {
                doc.DocumentNode.SelectSingleNode("//div[@class='content-border list']").Remove();
            }

            if (doc.DocumentNode.SelectSingleNode("//div[@id='categories_container']") != null)
            {
                doc.DocumentNode.SelectSingleNode("//div[@id='categories_container']").Remove();
            }

            if (doc.DocumentNode.SelectSingleNode("//div[@id='cookieFrame']") != null)
            {
                doc.DocumentNode.SelectSingleNode("//div[@id='cookieFrame']").Remove();
            }

            RelativeToAbsolute(doc, "//script");
            RelativeToAbsolute(doc, "//link");

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
            var requestUrl = string.Format(UrlBase + CleanCriteria(criteria), page);

            Log.Debug("Récupération des annonces à l'url [" + requestUrl + "]");

            var web = new HtmlWeb { OverrideEncoding = System.Text.Encoding.GetEncoding(Encoding) };
            var doc = web.Load(requestUrl);
            var links = doc.DocumentNode.SelectNodes("//div[@class='list-lbc']//a");

            return links == null ? null : links.ToList();
        }

        /// <summary>
        /// Replace relative link by absolute
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="xpathQuery">
        /// The xpath query.
        /// </param>
        private static void RelativeToAbsolute(HtmlDocument doc, string xpathQuery)
        {
            foreach (var node in doc.DocumentNode.SelectNodes(xpathQuery))
            {
                if (node.Attributes.Contains("src") && node.Attributes["src"].Value.StartsWith("/"))
                {
                    node.SetAttributeValue("src", UrlBase + node.Attributes["src"].Value);
                }

                if (node.Attributes.Contains("href") && node.Attributes["href"].Value.StartsWith("/"))
                {
                    node.SetAttributeValue("href", UrlBase + node.Attributes["href"].Value);
                }
            }
        }

/*
        /// <summary>
        /// Do ajax call to get phone gif url by replacing javascript call
        /// </summary>
        /// <param name="phoneLink">Clikable link to display phone number</param>
        /// <returns>Url of phone number gif</returns>
        private static string GetPhoneUrl(string phoneLink)
        {
            // Get param
            if (string.IsNullOrEmpty(phoneLink))
            {
                return null;
            }

            var startIndex = phoneLink.IndexOf('(') + 1;
            var functionParam = phoneLink.Substring(startIndex, phoneLink.Length - startIndex - 1);
            var param = functionParam.Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries);

            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(param[0].Replace("\"", string.Empty).Trim() + "/ajapi/get/phone?list_id=" + param[1].Trim());

                string json;
                using (var sr = new StreamReader(httpWebRequest.GetResponse().GetResponseStream()))
                {
                    json = sr.ReadToEnd();
                }

                if (!string.IsNullOrEmpty(json) && json != "\"\"")
                {
                    var obj = (JObject)JsonConvert.DeserializeObject(json);
                    return Convert.ToString(obj["phoneUrl"]);
                }
            }
            catch (Exception e)
            {
                Log.Error("Erreur lors de la récupération du téléphone à partir de [" + phoneLink + "]", e);
            }

            return null;
        }
*/
    }
}