using System;
using System.Collections.Generic;
using System.Text;

namespace Axis.Luna.Operation
{
    /// <summary>
    /// Determines the bias for the fold function on operations.
    /// </summary>
    public enum FoldBias
    {
        /// <summary>
        /// Instructs the algorithm to fail if any of the given operations failed
        /// </summary>
        Fail,

        /// <summary>
        /// Instructs the algorithm to pass if any of the given operations passed
        /// </summary>
        Pass
    }
}
