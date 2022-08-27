using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Axis.Luna.Extensions
{
    public static class Async
    {
        private static readonly ConcurrentDictionary<object, SemaphoreSlim> Locks = new ConcurrentDictionary<object, SemaphoreSlim>();

        /// <summary>
        /// Enables async locking in an async method
        /// </summary>
        /// <typeparam name="Result">The return value's type</typeparam>
        /// <param name="semaphore">the semaphore</param>
        /// <param name="asyncTask">the async task</param>
        /// <returns>the returned task</returns>
        public static async Task<Result> AsyncLock<Result>(this SemaphoreSlim semaphore, Func<Task<Result>> asyncTask)
        {
            await semaphore.WaitAsync();
            try
            {
                return await asyncTask.Invoke();
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Enables async locking in an async method
        /// </summary>
        /// <typeparam name="Result">The return value's type</typeparam>
        /// <param name="key">the key to lock on</param>
        /// <param name="asyncTask">the async task</param>
        /// <returns></returns>
        public static async Task<Result> AsyncLock<Result>(this object key, Func<Task<Result>> asyncTask)
        {
            var semaphore = Locks.GetOrAdd(key, _ => new SemaphoreSlim(1));

            try
            {
                return await semaphore.AsyncLock(asyncTask);
            }
            finally
            {
                _ = Locks.TryRemove(key, out var _);
            }
        }

        /// <summary>
        /// Enables async locking in an async method
        /// </summary>
        /// <param name="semaphore">the semaphore</param>
        /// <param name="asyncTask">the async task</param>
        /// <returns>the returned task</returns>
        public static async Task AsyncLock(this SemaphoreSlim semaphore, Func<Task> asyncTask)
        {
            await semaphore.WaitAsync();
            try
            {
                await asyncTask.Invoke();
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Enables async locking in an async method
        /// </summary>
        /// <param name="key">the key to lock on</param>
        /// <param name="asyncTask">the async task</param>
        /// <returns>the returned task</returns>
        public static async Task AsyncLock<Result>(this object key, Func<Task> asyncTask)
        {
            var semaphore = Locks.GetOrAdd(key, _ => new SemaphoreSlim(1));

            try
            {
                await semaphore.AsyncLock(asyncTask);
            }
            finally
            {
                _ = Locks.TryRemove(key, out var _);
            }
        }
    }
}
