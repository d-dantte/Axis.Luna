using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axis.Luna.Extensions
{
    public static class FileExtensions
    {
        /// <summary>
        /// Opens a file stream from a file info when it becomes available, using a timespan of 30 seconds.
        /// The classic usage scenario for this is with file watchers: for files of considerable size, or for system lags,
        /// a file may not be available to read even if a FileWatcher event has been fired for the file; this method will 
        /// continue to attempt to open the file until the specified timeout is reached.
        /// </summary>
        /// <param name="finfo"></param>
        /// <returns></returns>
        public static Stream OpenWhenAvailable(this FileInfo finfo) => finfo.OpenWhenAvailable(TimeSpan.FromSeconds(30));
        public static Stream OpenWhenAvailable(this FileInfo finfo, TimeSpan timeout)
        {
            var start = DateTime.Now;
            while (DateTime.Now - start <= timeout)
            {
                System.Threading.Thread.Sleep(50);
                try
                {
                    using (var fs = new FileStream(finfo.FullName, FileMode.Open))
                    {
                        var ms = new MemoryStream();
                        fs.CopyTo(ms);
                        ms.Position = 0;
                        return ms;
                    }
                }
                catch { }
            }
            return null;
        }
    }
}
