using System;
using System.Threading;
using System.Threading.Tasks;

namespace Axis.Luna.Extensions
{
    public static class Async
    {

        /// <summary>
        /// Enables async locking in an async method
        /// </summary>
        /// <typeparam name="Result"></typeparam>
        /// <param name="semaphore"></param>
        /// <param name="asyncTask"></param>
        /// <returns></returns>
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

        public static async Task<Result> AsyncLock<Result>(Func<Task<Result>> asyncTask) => await new SemaphoreSlim(1, 1).AsyncLock(asyncTask);
    }
}
