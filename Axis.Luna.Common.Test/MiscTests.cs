using Axis.Luna.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Axis.Luna.Common.Test
{
    public class SequencePage<Data>
    {
        /// <summary>
        /// A slice/page of the data returned by the query
        /// </summary>
        public Data[] Page { get; set; }

        /// <summary>
        /// The index of the current page. This index starts from 0
        /// </summary>
        public uint PageIndex { get; set; }

        /// <summary>
        /// The original size of a page requested. Note that the number of entries in the <c>Page</c> array is always <= this value.
        /// To illustrate this, imagine a sequence of 10 elements, requesting page with index 1 (the second page), where the page-size is 6...
        /// page 0 will contain the first 6 elements of the array, while page 1 will contain the last 4 elements of the array (4 <= 6).
        /// </summary>
        public uint PageSize { get; set; }

        /// <summary>
        /// The total length of the sequence from which the page is to be gotten
        /// </summary>
        public ulong? TotalLength { get; set; }

        /// <summary>
        /// Returns an array containing page indexes for pages immediately adjecent to the current page.
        /// The span indicates how many pages indexes to each side of the current page should be returned
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public ulong[] AdjacentIndexes(ulong span)
        {
            if (TotalLength == null)
                return new ulong[0];

            var fullspan = (span * 2) + 1;
            ulong start = 0,
                  count = 0;

            var pageCount = TotalLength.Value / PageSize + (ulong)(TotalLength.Value % PageSize > 0 ? 1 : 0);
            if (fullspan >= pageCount) count = pageCount;

            else
            {
                start = PageIndex - span;
                count = fullspan;

                if (start < 0) start = 0;
                if ((PageIndex + span) >= pageCount) start = pageCount - fullspan;
            }

            return this
                .GenerateSequence(count, _indx => _indx + start)
                .ToArray();
        }

        public bool IsValidPage()
        {
            if (Page == null) return false;

            else if (PageSize == 0) return false;

            else if (TotalLength.HasValue)
            {
                if ((ulong)Page.Length > TotalLength.Value)
                    return false;

                var pageCount = TotalLength.Value / PageSize + (ulong)(TotalLength.Value % PageSize > 0 ? 1 : 0);
                if (PageIndex >= pageCount)
                    return false;

                if (!IsLastPage() && (ulong)Page.Length < pageCount)
                    return false;
            }

            return true;
        }

        public bool IsLastPage()
        {
            var pageCount = TotalLength.Value / PageSize + (ulong)(TotalLength.Value % PageSize > 0 ? 1 : 0);
            return pageCount == PageIndex - 1;
        }

        public bool IsFirstPage() => PageIndex == 0;


        private IEnumerable<V> GenerateSequence<V>(ulong repetitions, Func<ulong, V> generator)
        {
            for (ulong cnt = 0; cnt < repetitions; cnt++)
                yield return generator.Invoke(cnt);
        }

        public static bool isTwin(string a, string b)
        {
            if (a.Length != b.Length)
                return false;

            var compositionA = a
                .GroupBy(c => c)
                .ToDictionary(
                    group => group.Key,
                    group => group.Count());

            var compositionB = b
                .GroupBy(c => c)
                .ToDictionary(
                    group => group.Key,
                    group => group.Count());

            if (compositionA.Count != compositionB.Count)
                return false;

            foreach(var kvp in compositionA)
            {
                if (!compositionB.ContainsKey(kvp.Key) || compositionB[kvp.Key] != kvp.Value)
                    return false;
            }

            return true;
        }

        public static string Reshape(int n, string str)
        {
            var groups = str
                .Replace(" ", "")
                .GroupBy((chr, index) => index / n)
                .Select(group => new string(group.ToArray()))
                .ToArray();

            return string.Join("\n", groups);
        }
    }

    [TestClass]
    public class MiscTests
    {
        [TestMethod]
        public void t()
        {
            var dict = new Dictionary<string, string>();
            Console.WriteLine(dict.TryAdd("me", "you"));
            Console.WriteLine(dict.TryAdd("me", "not you"));
            Console.WriteLine(dict["me"]);
        }

        [TestMethod]
        public void Test()
        {
            var page = new SequencePage<int>
            {
                Page = new int[10],
                PageIndex = 5,
                PageSize = 10,
                TotalLength = 200
            };
            var pages = page.AdjacentIndexes(2);
            Console.WriteLine(string.Join(',', pages));
        }

        [TestMethod]
        public void ConcurrentDictionarySemaphoreTest()
        {
            var xmaphore = new ConcurrentDictionary<string, string>();
            
            var t = new Thread(() =>
            {
                xmaphore.SemaphoreLock("abcd", k =>
                {
                    Console.WriteLine("Entered");
                    Thread.Sleep(200);
                    Console.WriteLine("Woken up");
                });
            });

            t.Start();

            Thread.Yield();
            xmaphore.SemaphoreLock(
                key: "abcd",
                granted: k => Console.WriteLine("granted"),
                denied: k => Console.WriteLine("denied"));


            Thread.Sleep(1000);
        }

        [TestMethod]
        public void TestMethod5()
        {
            var s = new Lazy<int>(() => 6);
            var f = new Lazy<int>(() => new Exception().Throw<int>());

            try
            {
                _ = s.Value;
                _ = f.Value;
            }
            catch { }
        }

        [TestMethod]
        public void ObjectIDGenerator_Tests()
        {
            var idGen = new ObjectIDGenerator();

            var obj = new object();
            var obj2 = new object();

            var id1 = idGen.GetId(obj, out var added);
            var id2 = idGen.GetId(obj, out added);
            var id3 = idGen.GetId(obj2, out added);

            Assert.ThrowsException<ArgumentNullException>(() => idGen.GetId(null, out _));

        }
    }

    public static class XYZExtensions
    {
        internal static TResult SemaphoreLock<TLock, TResult>(this
            ConcurrentDictionary<TLock, TLock> dictionary,
            TLock key,
            Func<TLock, TResult> granted,
            Func<TLock, TResult> denied = null)
        {
            if (dictionary.TryAdd(key, key))
            {
                try
                {
                    return granted.Invoke(key);
                }
                finally
                {
                    dictionary.TryRemove(key, out _);
                }
            }

            else if (denied != null)
                return denied.Invoke(key);

            else
                return default;
        }

        internal static void SemaphoreLock<TLock>(this
            ConcurrentDictionary<TLock, TLock> dictionary,
            TLock key,
            Action<TLock> granted,
            Action<TLock> denied = null)
        {
            if (dictionary.TryAdd(key, key))
            {
                try
                {
                    granted.Invoke(key);
                }
                finally
                {
                    dictionary.TryRemove(key, out _);
                }
            }

            else
                denied?.Invoke(key);
        }
    }
}
