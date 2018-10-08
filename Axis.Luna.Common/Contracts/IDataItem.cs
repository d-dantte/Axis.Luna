using System.Collections.Generic;

namespace Axis.Luna.Common.Contracts
{
    public interface IDataItem
    {
        string Name { get; set; }

        string Data { get; set; }

        CommonDataType Type { get; set; }

        string DisplayData();

        /// <summary>
        /// An array of values interpreted by the IDataItem implementation. Each implementation determines how the tuples are ordered, so each value can be 
        /// read and parsed accordingly.
        /// </summary>
        /// <param name="tuples"></param>
        void Initialize(string[] tuples);

        /// <summary>
        /// Converts the relevant information from the implementation into "tuples" with values placed in specific positions
        /// </summary>
        /// <returns></returns>
        string[] Tupulize();
    }
}
