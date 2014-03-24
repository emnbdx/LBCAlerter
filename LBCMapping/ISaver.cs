namespace LBCMapping
{
    /// <summary>
    /// Implement this interface to make your own save system
    /// </summary>
    public interface ISaver
    {
        /// <summary>
        /// Sercher use this method to know if continue search or not
        /// </summary>
        /// <param name="ad">Ad to store, unique ID is ad.adUrl</param>
        /// <returns>True if store success (not already existing Ad) else false</returns>
        bool Store(Ad ad);
    }
}