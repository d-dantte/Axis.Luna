using static Axis.Luna.Extensions.EnumerableExtensions;
using static Axis.Luna.Extensions.ObjectExtensions;
using static Axis.Luna.Extensions.ExceptionExtensions;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Axis.Luna.Operation;

namespace Axis.Luna.Utils
{
    public class AssemblyMonitor
    {

        private List<Action<Assembly>> _callBacks = new List<Action<Assembly>>();
        private DirectoryInfo _bin = null;
        private Dictionary<string, FileSystemWatcher> _watchers = new Dictionary<string, FileSystemWatcher>();
        private string[] _filters = null;

        //private static List<Assembly> _loadedAssemblies = new List<Assembly>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="binDirectory"></param>
        /// <param name="filters">additional comma separated file filters to load assemblies from, separate from .dll and .exe</param>
        public AssemblyMonitor(DirectoryInfo binDirectory, string filters = null)
        {
            //ultimately, hook a reflection only resolver 
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += (s, a) => Assembly.ReflectionOnlyLoad(a.Name);

            filters = filters ?? "";
            this._bin = new DirectoryInfo(binDirectory.FullName);
            (_filters = ("*.dll,*.exe" + filters).Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                                                 .ForAll((cnt, filter) => _watchers.Add(filter, new FileSystemWatcher
                                                 {
                                                     Filter = filter,
                                                     Path = _bin.FullName,
                                                     NotifyFilter = NotifyFilters.FileName,
                                                     EnableRaisingEvents = true,
                                                     IncludeSubdirectories = true
                                                 }));

            _watchers.Values.ForAll((cnt, watcher) => watcher.Created += Watcher_Changed);
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        => Task.Run(() =>
        {
            var asm = Assembly.ReflectionOnlyLoadFrom(e.FullPath);
            List<Action<Assembly>> cbs = null;
            lock (_callBacks) cbs = _callBacks.ToList();
            cbs.ForEach(cb => ResolvedOp.Try(() => cb(asm)));
        });

        /// <summary>
        /// Attach a callback that gets notified with a Reflection-Only-Loaded Assembly
        /// </summary>
        /// <param name="callBack"></param>
        /// <param name="rescan"></param>
        /// <returns></returns>
        public AssemblyMonitor AttachMonitor(Action<Assembly> callBack, bool rescan = false)
        {
            ThrowNullArguments(() => callBack);

            lock (_callBacks) _callBacks.Add(callBack);
            if (rescan)
            {
                _filters.Select(flt => _bin.EnumerateFiles(flt, SearchOption.AllDirectories))
                        .SelectMany(fileGroups => fileGroups)
                        .ForAll((cnt, file) => Watcher_Changed(null, new FileSystemEventArgs(WatcherChangeTypes.Created, file.Directory.FullName, file.Name)));
            }

            return this;
        }
    }
}
