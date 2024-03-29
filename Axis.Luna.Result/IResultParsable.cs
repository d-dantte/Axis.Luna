﻿
namespace Axis.Luna.Result
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TSelf"></typeparam>
    public interface IResultParsable<TSelf> where TSelf : IResultParsable<TSelf>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        abstract static bool TryParse(string text, out IResult<TSelf> result);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        abstract static IResult<TSelf> Parse(string text);
    }
}