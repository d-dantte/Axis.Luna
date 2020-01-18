using System.Collections.Generic;

namespace Axis.Luna.Common.Contracts
{
    public interface IDataItem
    {
        string Name { get; set; }

        string Data { get; set; }

        CommonDataType Type { get; set; }

        //string DisplayData();

        /// <summary>
        /// An array of values interpreted by the IDataItem implementation. Each implementation determines how the tuples are ordered, so each value can be 
        /// read and parsed accordingly. Basically, this method reconstructs this IDataItem from the tuple.
        /// </summary>
        /// <param name="tuple"></param>
        void Initialize(string[] tuple);

        /// <summary>
        /// Converts the relevant information from the implementation into "tuples" with values placed in specific positions.
        /// Basically, this method breaks the information contained within the tag into disparate elements, together forming a
        /// tuple.
        /// </summary>
        /// <returns></returns>
        string[] Tupulize();


        #region Forced Overrides
        bool Eqals(object item);
        int GetHashCode();
        #endregion
    }
}
