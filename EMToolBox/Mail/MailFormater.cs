﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MailFormater.cs" company="Eddy MONTUS">
//   2014
// </copyright>
// <summary>
//   Defines the MailFormater type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace EMToolBox.Mail
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The mail formatter.
    /// </summary>
    public class MailFormatter
    {
        /// <summary>
        /// The regex value.
        /// </summary>
        private static readonly Regex RegValue = new Regex(@"({)([^}]+)(})", RegexOptions.IgnoreCase);

        /// <summary>
        /// The regex conditional.
        /// </summary>
        private static readonly Regex RegConditional = new Regex(@"\[(?<tag>\w.*)\](?<text>.*)\[/\k<tag>\]", RegexOptions.IgnoreCase);

        /// <summary>
        /// The regex duplicate.
        /// </summary>
        private static readonly Regex RegDuplicate = new Regex(@"\#(?<tag>\w.*)\#(?<text>.*)\#/\k<tag>\#", RegexOptions.IgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="MailFormatter"/> class.
        /// </summary>
        /// <param name="pattern">
        /// The pattern.
        /// </param>
        /// <param name="source">
        /// The source.
        /// </param>
        public MailFormatter(string pattern, object source)
        {
            if (source == null)
            {
                return;
            }

            if (source is Dictionary<string, object>)
            {
                this.Formatted = FromDictionary(pattern, source as Dictionary<string, object>);
            }
            else if (source is string)
            {
                this.Formatted = FromJson(pattern, source as string);
            }
        }

        /// <summary>
        /// Gets the formatted.
        /// </summary>
        public string Formatted { get; private set; }

        /// <summary>
        /// The from dictionary.
        /// </summary>
        /// <param name="pattern">
        /// The pattern.
        /// </param>
        /// <param name="parameters">
        /// The parameters.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string FromDictionary(string pattern, Dictionary<string, object> parameters)
        {
            var output = pattern;

            foreach (var key in parameters.Keys)
            {
                var tmp = parameters[key];

                string result;
                if (tmp is int)
                {
                    result = Convert.ToInt32(tmp).ToString(CultureInfo.InvariantCulture);
                }
                else if (tmp is decimal)
                {
                    result = Convert.ToDecimal(tmp).ToString(CultureInfo.InvariantCulture);
                }
                else if (tmp is DateTime)
                {
                    result = Convert.ToDateTime(tmp).ToString("dd/MM/yyyy HH:mm:ss");
                }
                else if (tmp is bool)
                {
                    result = Convert.ToBoolean(tmp) ? "Oui" : "Non";
                }
                else
                {
                    result = Convert.ToString(tmp);
                }

                output = output.Replace(key, result);
            }

            return output;
        }

        /// <summary>
        /// The from JSON.
        /// </summary>
        /// <param name="pattern">
        /// The pattern.
        /// </param>
        /// <param name="json">
        /// The JSON as string.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string FromJson(string pattern, string json)
        {
            var output = pattern;

            var jsonObject = JObject.Parse(json);
            output = FormatConditionalBlock(jsonObject, output);
            output = FormatDuplicate(jsonObject, output);
            output = FormatValue(jsonObject, output);

            return output;
        }

        /// <summary>
        /// Get value in JSON string 
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="tag">
        /// The tag.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>IEnumerable</cref>
        ///     </see>
        ///     .
        /// </returns>
        private static IEnumerable<string> GetValueFromJson(JToken source, string tag)
        {
            var query = tag.Split(".".ToCharArray());
            IEnumerable<JToken> tokens;
            var result = new List<string>();
            if (query.Count() == 1)
            {
                query = tag.Split(">".ToCharArray());

                if (query.Count() == 1)
                {
                    tokens = source.SelectTokens(query[0]);
                    result.AddRange(tokens.Select(token => (string)token));
                }
                else
                {
                    tokens = source.SelectTokens(query[0] + "." + query[1]);
                    result.AddRange(tokens.Select(token => (string)token));
                }
            }
            else
            {
                tokens = source.SelectToken(query[0]).Where(x => x["Type"].ToString() == query[1]);
                result.AddRange(tokens.Select(token => (string)token["Value"]));
            }

            return result;
        }

        /// <summary>
        /// Keep or erase block, this is used for boolean parameter
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="format">
        /// The format.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string FormatConditionalBlock(JToken source, string format)
        {
            var mc = RegConditional.Matches(format);
            foreach (Match m in mc)
            {
                // Get param name 
                var paramName = m.Groups["tag"].Value;
                var nega = paramName.StartsWith("NOT_");
                paramName = paramName.Replace("NOT_", string.Empty);

                var result = GetValueFromJson(source, paramName).FirstOrDefault();

                if (!nega)
                {
                    if (!string.IsNullOrEmpty(result) && result != "False")
                    {
                        format = format.Replace(m.Groups[0].Value, m.Groups[2].Value);
                    }
                    else
                    {
                        format = format.Replace(m.Groups[0].Value, string.Empty);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(result) || result == "False")
                    {
                        format = format.Replace(m.Groups[0].Value, m.Groups[2].Value);
                    }
                    else
                    {
                        format = format.Replace(m.Groups[0].Value, string.Empty);
                    }
                }
            }

            return format;
        }

        /// <summary>
        /// Replace block and duplicate it, this is used for array parameter
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="format">
        /// The a format.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string FormatDuplicate(JToken source, string format)
        {
            var mc = RegDuplicate.Matches(format);
            foreach (Match m in mc)
            {
                // Get param name 
                var paramName = m.Groups["tag"].Value;

                var results = GetValueFromJson(source, paramName);

                var toDuplicate = m.Groups[2].Value;

                var duplicate = results.Aggregate(string.Empty, (current, value) => current + toDuplicate.Replace("{" + paramName + "}", value));

                format = format.Replace(m.Groups[0].Value, duplicate);
            }

            return format;
        }

        /// <summary>
        /// Format value only, simple replacement
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="format">
        /// The a format.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string FormatValue(JToken source, string format)
        {
            var sb = new StringBuilder();

            var mc = RegValue.Matches(format);
            var startIndex = 0;
            foreach (Match m in mc)
            {
                var g = m.Groups[2]; // it's second in the match between { and }
                var length = g.Index - startIndex - 1;
                sb.Append(format.Substring(startIndex, length));

                var result = GetValueFromJson(source, g.Value).FirstOrDefault();

                if (!string.IsNullOrEmpty(result))
                {
                    sb.Append(result);
                }
                else
                {
                    sb.Append("{" + g.Value + "}");
                }

                startIndex = g.Index + g.Length + 1;
            }

            if (startIndex < format.Length)
            {
                // include the rest (end) of the string
                sb.Append(format.Substring(startIndex));
            }

            return sb.ToString();
        }
    }
}
