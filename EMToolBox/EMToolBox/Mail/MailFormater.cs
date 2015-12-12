// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MailFormater.cs" company="Eddy MONTUS">
//   2014
// </copyright>
// <summary>
//   Defines the MailFormater type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace EMToolBox.Mail
{
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
        /// <param name="data">
        /// The data as JSON representation.
        /// </param>
        public MailFormatter(string pattern, string data)
        {
            var output = pattern;

            var jsonObject = JObject.Parse(data);
            output = FormatConditionalBlock(jsonObject, output);
            output = FormatDuplicate(jsonObject, output);
            output = FormatValue(jsonObject, output);

            this.Formatted = output;
        }

        /// <summary>
        /// Gets the formatted.
        /// </summary>
        public string Formatted { get; private set; }

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

                var result = source.GetTokenValue(paramName).FirstOrDefault();

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

                var results = source.GetTokenValue(paramName);

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

                var result = source.GetTokenValue(g.Value).FirstOrDefault();

                if (!string.IsNullOrEmpty(result))
                {
                    sb.Append(result);
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
