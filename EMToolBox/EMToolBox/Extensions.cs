// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Extensions.cs" company="Eddy MONTUS">
//   2014
// </copyright>
// <summary>
//   The extensions.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace EMToolBox
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The extensions.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// The get tag value in JSON JToken.
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
        public static IEnumerable<string> GetTokenValue(this JToken source, string tag)
        {
            var query = tag.Split(">".ToCharArray());
            IEnumerable<JToken> tokens;
            var result = new List<string>();
            if (query.Count() == 1)
            {
                tokens = source.SelectTokens(query[0]);
                result.AddRange(tokens.Select(token => (string)token));
            }
            else
            {
                tokens = source.SelectToken(query[0]).Where(x => x["Type"].ToString() == query[1]);
                result.AddRange(tokens.Select(token => (string)token["Value"]));
            }

            return result;
        }

        /// <summary>
        /// The get JToken in JSON JToken.
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
        public static IEnumerable<JToken> GetToken(this JToken source, string tag)
        {
            var query = tag.Split(">".ToCharArray());

            return query.Count() == 1 ? source.SelectTokens(query[0]) : source.SelectToken(query[0]).Where(x => x["Type"].ToString() == query[1]);
        }

        /// <summary>
        /// The get md 5 hash.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetMd5Hash(this string input)
        {
            using (var md5Hash = MD5.Create())
            {
                var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                var result = new StringBuilder();
                foreach (var t in data)
                {
                    result.Append(t.ToString("x2"));
                }

                return result.ToString();
            }
        }
    }
}
