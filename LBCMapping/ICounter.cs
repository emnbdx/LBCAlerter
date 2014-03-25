using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBCMapping
{
    /// <summary>
    /// Implement this interface to make your own log system
    /// </summary>
    public interface ICounter
    {
        void Count();

        void Result(int count);
    }
}
