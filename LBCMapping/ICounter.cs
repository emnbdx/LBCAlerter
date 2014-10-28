namespace LBCMapping
{
    /// <summary>
    /// Implement this interface to make your own log system
    /// </summary>
    public interface ICounter
    {
        /// <summary>
        /// The count.
        /// </summary>
        void Count();

        /// <summary>
        /// The result.
        /// </summary>
        /// <param name="count">
        /// The count.
        /// </param>
        void Result(int count);
    }
}
