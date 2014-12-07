// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAlerter.cs" company="Eddy MONTUS">
//   2014
// </copyright>
// <summary>
//   Implement this interface to make your own alert system
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LBCMapping
{
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Implement this interface to make your own alert system
    /// </summary>
    public interface IAlerter
    {
        /// <summary>
        /// The alert.
        /// </summary>
        /// <param name="ad">
        /// The ad.
        /// </param>
        void Alert(JObject ad);
    }
}