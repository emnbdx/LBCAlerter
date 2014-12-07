// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISaver.cs" company="Eddy MONTUS">
//   2014
// </copyright>
// <summary>
//   Implement this interface to make your own save system
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LBCMapping
{
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Implement this interface to make your own save system
    /// </summary>
    public interface ISaver
    {
        /// <summary>
        /// Searcher use this method to know if continue search or not
        /// </summary>
        /// <param name="ad">Ad to store, in JSON representation</param>
        /// <returns>True if store success (not already existing Ad) else false</returns>
        bool Store(JObject ad);
    }
}