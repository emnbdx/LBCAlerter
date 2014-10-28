namespace LBCMapping
{
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
        void Alert(Ad ad);
    }
}