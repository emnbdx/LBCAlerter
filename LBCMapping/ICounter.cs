// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICounter.cs" company="Eddy MONTUS">
//   2014
// </copyright>
// <summary>
//   Implement this interface to make your own log system
// </summary>
// --------------------------------------------------------------------------------------------------------------------

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
