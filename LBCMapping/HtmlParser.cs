namespace LBCMapping
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;

    using EMToolBox;

    using HtmlAgilityPack;
    using log4net;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

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
        /// The remove nodes.
        /// </summary>
        private static readonly List<string> RemoveNodes = new List<string>
                                                               {
                                                                   "//table[@id='TableContentTop']",
                                                                   "//span[@class='SeparatorText']//..",
                                                                   "//div[@class='Deposer']",
                                                                   "//div[@id='incr_renc_home_button']",
                                                                   "//div[@id='Footer']",
                                                                   "//div[@id='Banner_sky']",
                                                                   "//div[@id='oas-top1']",
                                                                   "//div[@id='cookieFrame']",
                                                                   "//div[@id='account_login_f']",
                                                                   "//header[@id='headermain']",
                                                                   "//div[@class='oas-x01']",
                                                                   "//div[@class='oas-x02']",
                                                                   "//div[@class='oas-x03']",
                                                                   "//div[@class='oas-x04']",
                                                                   "//div[@class='oas-top']",
                                                                   "//div[@id='account_submenu']",
                                                                   "//div[@class='comment']",
                                                                   "//div[@class='content-border list']",
                                                                   "//div[@id='categories_container']",
                                                                   "//nav",
                                                               };

        /// <summary>
        /// The remove sripts.
        /// </summary>
        private static readonly List<string> RemoveSripts = new List<string>
                                                                {
                                                                    "cedexis",
                                                                    "xiti",
                                                                };

        /// <summary>
        /// The remove styles.
        /// </summary>
        private static readonly List<string> RemoveStyles = new List<string>
                                                                {
                                                                    "//body[@id='all']",
                                                                    "//div[@id='page_width']",
                                                                    "//div[@class='search_box']"
                                                                };

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
        public static JObject ExtractAdInformation(HtmlNode link)
        {
            var ad = link.SelectSingleNode("div[@class='lbc']");

            // Get all value
            var dateNode = ad.SelectNodes("div[@class='date']/div");
            var titleNode = ad.SelectSingleNode("div[@class='detail']/h2[@class='title']");
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

            var sb = new StringBuilder();
            var sw = new StringWriter(sb);
            using (var writer = new JsonTextWriter(sw))
            {
                writer.WriteStartObject();

                writer.WritePropertyName("Url");
                writer.WriteValue(link.GetAttributeValue("href", string.Empty));
                writer.WritePropertyName("Date");
                writer.WriteValue(realDate.ToString(new CultureInfo("fr-FR")));
                writer.WritePropertyName("Title");
                writer.WriteValue(titleNode != null ? titleNode.InnerText.Trim() : string.Empty);

                writer.WritePropertyName("Contents");
                writer.WriteStartArray();

                writer.WriteStartObject();
                writer.WritePropertyName("Type");
                writer.WriteValue("PictureUrl");
                writer.WritePropertyName("Value");
                writer.WriteValue(imgNode != null ? imgNode.GetAttributeValue("src", string.Empty).Replace("thumbs", "images") : string.Empty);
                writer.WriteEndObject();

                writer.WriteStartObject();
                writer.WritePropertyName("Type");
                writer.WriteValue("Place");
                writer.WritePropertyName("Value");
                writer.WriteValue(placementNode != null ? placementNode.InnerText.Replace("\r", string.Empty).Replace("\n", string.Empty).Replace(" ", string.Empty) : string.Empty);
                writer.WriteEndObject();

                writer.WriteStartObject();
                writer.WritePropertyName("Type");
                writer.WriteValue("Price");
                writer.WritePropertyName("Value");
                writer.WriteValue(priceNode != null ? priceNode.InnerText.Trim() : string.Empty);
                writer.WriteEndObject();

                writer.WriteEndArray();

                writer.WriteEndObject();
            }

            return JObject.Parse(sb.ToString());
        }

        /// <summary>
        /// The extract all ad information.
        /// </summary>
        /// <param name="ad">
        /// The ad.
        /// </param>
        /// <returns>
        /// The <see cref="JObject"/>.
        /// </returns>
        public static JObject ExtractAllAdInformation(JObject ad)
        {
            var web = new HtmlWeb { OverrideEncoding = System.Text.Encoding.GetEncoding(Encoding) };
            var doc = web.Load(ad["Url"].ToString());
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

            //HtmlNode phoneNode = adContent.SelectSingleNode("//span[@class='lbcPhone']/span[@id='phoneNumber']/a");
            var commercialNode = content.SelectSingleNode("//div[@class='lbc_nosalesmen']");
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
                    /*if (parameter.Attributes["itemprop"] != null && parameter.Attributes["itemprop"].Value == "geo")
                    {
                        latitudeNode = parameter.SelectSingleNode("//td//meta[@itemprop='latitude']");
                        longitudeNode = parameter.SelectSingleNode("//td//meta[@itemprop='longitude']");
                    }
                    else
                    {*/

                    if (parameter.SelectSingleNode("th") == null)
                    {
                        continue;
                    }

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

            var positionScript = content.SelectSingleNode("//div[@class='colRight']/script");
            string latitudeNode = null;
            string longitudeNode = null;
            var ville = true;
            if (positionScript != null)
            {
                var regex = new Regex(
                    "var\\s(\\w*)\\s=.*(?:\\\"(.*)\\\").*",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace
                    | RegexOptions.Compiled);

                var values = positionScript.InnerText.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                foreach (var match in values.Select(s => regex.Match(s)))
                {
                    if (match.Groups.Count < 3)
                    {
                        continue;
                    }

                    if (match.Groups[1].Value == "lat")
                    {
                        latitudeNode = match.Groups[2].Value.Trim();
                    }
                    if (match.Groups[1].Value == "lng")
                    {
                        longitudeNode = match.Groups[2].Value.Trim();
                    }
                    if (match.Groups[1].Value == "source")
                    {
                        ville = match.Groups[2].Value.Trim() != "address";
                    }
                }
            }

            var descriptionNode = content.SelectSingleNode("//div[@class='AdviewContent']/div[@class='content']");


            using (var writer = ((JArray)ad["Contents"]).CreateWriter())
            {
                var existingPictures = ad.GetTokenValue("Contents>PictureUrl");
                foreach (var picture in pictures.Where(picture => !existingPictures.Contains(picture)))
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("Type");
                    writer.WriteValue("PictureUrl");
                    writer.WritePropertyName("Value");
                    writer.WriteValue(picture);
                    writer.WriteEndObject();
                }

                // TODO : find good solution to get phone
                // ad.AddContentsToToken("Phone", phoneNode != null ? GetPhoneUrl(phoneNode.GetAttributeValue("href", string.Empty)) : string.Empty), "Contents");

                writer.WriteStartObject();
                writer.WritePropertyName("Type");
                writer.WriteValue("AllowCommercial");
                writer.WritePropertyName("Value");
                writer.WriteValue(commercialNode == null);
                writer.WriteEndObject();

                writer.WriteStartObject();
                writer.WritePropertyName("Type");
                writer.WriteValue("Name");
                writer.WritePropertyName("Value");
                writer.WriteValue(nameNode != null ? nameNode.InnerText : string.Empty);
                writer.WriteEndObject();

                writer.WriteStartObject();
                writer.WritePropertyName("Type");
                writer.WriteValue("ContactUrl");
                writer.WritePropertyName("Value");
                writer.WriteValue(emailNode != null ? emailNode.GetAttributeValue("href", string.Empty) : string.Empty);
                writer.WriteEndObject();

                foreach (var parameter in parameters)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("Type");
                    writer.WriteValue("Param");
                    writer.WritePropertyName("Value");
                    writer.WriteValue(parameter);
                    writer.WriteEndObject();
                }

                writer.WriteStartObject();
                writer.WritePropertyName("Type");
                writer.WriteValue("Description");
                writer.WritePropertyName("Value");
                writer.WriteValue(descriptionNode != null ? descriptionNode.InnerHtml : string.Empty);
                writer.WriteEndObject();

                if (ville)
                {
                    return ad;
                }

                writer.WriteStartObject();
                writer.WritePropertyName("Type");
                writer.WriteValue("Latitude");
                writer.WritePropertyName("Value");
                writer.WriteValue(latitudeNode);
                writer.WriteEndObject();

                writer.WriteStartObject();
                writer.WritePropertyName("Type");
                writer.WriteValue("Longitude");
                writer.WritePropertyName("Value");
                writer.WriteValue(longitudeNode);
                writer.WriteEndObject();
            }

            return ad;
        }

        /// <summary>
        /// Return clean page
        /// </summary>
        /// <param name="type">Page type</param>
        /// <param name="path">Criteria path from home page (contain region)</param>
        /// <returns>Html code of page</returns>
        public static string GetPage(PageType type, string path = null)
        {
            var url = string.Empty;
            switch (type)
            {
                case PageType.Home:
                    url = UrlBase;
                    break;
                case PageType.Criteria:
                    if (string.IsNullOrEmpty(path))
                    {
                        throw new Exception("path is needed for criteria page");
                    }

                    url = path.StartsWith(UrlBase) ? string.Format(CleanCriteria(path), 0) : string.Format(UrlBase + CleanCriteria(path), 0);
                    break;
            }
            
            var web = new HtmlWeb { OverrideEncoding = System.Text.Encoding.GetEncoding(Encoding) };
            var doc = web.Load(url);

            RemoveUnusedDiv(doc);
            RemoveBackground(doc);
            RemoveScript(doc);

            RelativeToAbsolute(doc, "//script");
            RelativeToAbsolute(doc, "//link");

            var tmp = doc.DocumentNode.WriteTo();
            return tmp;
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
            var links = doc.DocumentNode.SelectNodes("//div[@class='list-lbc']/a");

            return links == null ? null : links.ToList();
        }

        /// <summary>
        /// The get specific content.
        /// </summary>
        /// <param name="pageType">
        /// The page type.
        /// </param>
        /// <param name="contentType">
        /// The content type.
        /// </param>
        /// <param name="path">
        /// The path.
        /// </param>
        /// <exception cref="Exception">
        /// If parameter not coherent
        /// </exception>
        /// <returns>
        /// The <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        public static List<string> GetSpecificContent(PageType pageType, ContentType contentType, string path = null)
        {
            var url = string.Empty;
            switch (pageType)
            {
                case PageType.Home:
                    url = UrlBase;
                    break;
                case PageType.Criteria:
                    if (string.IsNullOrEmpty(path))
                    {
                        throw new Exception("path is needed for criteria page");
                    }

                    url = path.StartsWith(UrlBase) ? string.Format(CleanCriteria(path), 0) : string.Format(UrlBase + CleanCriteria(path), 0);
                    break;
            }

            var web = new HtmlWeb { OverrideEncoding = System.Text.Encoding.GetEncoding(Encoding) };
            var doc = web.Load(url);

            RemoveScript(doc);

            foreach (var node in doc.DocumentNode.SelectNodes("//comment()"))
            {
                node.Remove();
            }

            var results = new List<string>();
            switch (contentType)
            {
                case ContentType.Style:
                    {
                        RelativeToAbsolute(doc, "//link");
                        var nodes = doc.DocumentNode.SelectNodes("//link[@type='text/css'] | //style");

                        foreach (var node in nodes)
                        {
                            if (node.Attributes["href"] != null)
                            {
                                // Download content
                                var httpWebRequest =
                                    (HttpWebRequest)WebRequest.Create(node.Attributes["href"].Value);

                                try
                                {
                                    string style;
                                    using (
                                        var sr = new StreamReader(httpWebRequest.GetResponse().GetResponseStream(),
                                            System.Text.Encoding.GetEncoding(Encoding)))
                                    {
                                        style = sr.ReadToEnd();
                                    }

                                    results.Add("<style type=\"text/css\">\r\n" + style + "\r\n</style>");
                                }
                                catch (WebException e)
                                {
                                }
                            }
                            else
                            {
                                results.Add(node.WriteTo());
                            }
                        }
                        break;
                    }

                case ContentType.Script:
                    {
                        RelativeToAbsolute(doc, "//script");
                        var nodes = doc.DocumentNode.SelectNodes("//script");

                        foreach (var node in nodes)
                        {
                            if (node.Attributes["src"] != null)
                            {
                                // Download content
                                var httpWebRequest =
                                    (HttpWebRequest)WebRequest.Create(node.Attributes["src"].Value);

                                try
                                {
                                    string script;
                                    using (
                                        var sr = new StreamReader(httpWebRequest.GetResponse().GetResponseStream(),
                                            System.Text.Encoding.GetEncoding(Encoding)))
                                    {
                                        script = sr.ReadToEnd();
                                    }

                                    results.Add("<script type=\"text/javascript\">\r\n" + script + "\r\n</script>");
                                }
                                catch (WebException e)
                                {
                                }
                            }
                            else
                            {
                                results.Add(node.WriteTo());
                            }
                        }

                        break;
                    }

                case ContentType.Body:
                    {
                        RemoveUnusedDiv(doc);
                        var nodes = doc.DocumentNode.SelectNodes("//body");

                        // body only one node
                        foreach (var node in nodes[0].SelectNodes("//script"))
                        {
                            node.Remove();
                        }

                        results.AddRange(nodes.Select(node => node.WriteTo()));
                        break;
                    }
            }

            return results;
        }

        /// <summary>
        /// The remove unused div.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        private static void RemoveUnusedDiv(HtmlDocument doc)
        {
            foreach (var node in RemoveNodes.Where(removeNode => doc.DocumentNode.SelectSingleNode(removeNode) != null).SelectMany(removeNode => doc.DocumentNode.SelectNodes(removeNode)))
            {
                node.Remove();
            }
        }

        /// <summary>
        /// The remove background.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        private static void RemoveBackground(HtmlDocument doc)
        {
            foreach (var removeStyle in RemoveStyles.Where(removeStyle => doc.DocumentNode.SelectSingleNode(removeStyle) != null))
            {
                if (doc.DocumentNode.SelectSingleNode(removeStyle).Attributes["style"] != null)
                {
                    doc.DocumentNode.SelectSingleNode(removeStyle).Attributes["style"].Value += "background-color: transparent;";
                }
                else
                {
                    doc.DocumentNode.SelectSingleNode(removeStyle).Attributes.Add("style", "background-color: transparent;");
                }
            }
        }

        /// <summary>
        /// The remove script.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        private static void RemoveScript(HtmlDocument doc)
        {
            foreach (var node in RemoveSripts.Select(removeSript => doc.DocumentNode.SelectNodes("//script[contains(@src, '" + removeSript + "')]")).Where(nodes => nodes != null).SelectMany(nodes => nodes))
            {
                node.Remove();
            }
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
                if (node.Attributes.Contains("src") && node.Attributes["src"].Value.StartsWith("//"))
                {
                    node.SetAttributeValue("src", "http:" + node.Attributes["src"].Value);
                }

                if (node.Attributes.Contains("href") && node.Attributes["href"].Value.StartsWith("//"))
                {
                    node.SetAttributeValue("href", "http:" + node.Attributes["href"].Value);
                }
                
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