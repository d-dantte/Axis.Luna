using System;
using System.Collections.Generic;

namespace Axis.Luna
{
    public interface IServiceResolver : IDisposable
    {
        object Resolve(Type serviceType, params object[] args);

        Service Resolve<Service>(params object[] args);

        IEnumerable<object> ResolveAll(Type serviceType, params object[] args);

        IEnumerable<Service> ResolveAll<Service>(params object[] args);

        /// <summary>
        /// Creates a new IServiceResolver that should be scopped to a transaction possibly specified via the parameter
        /// </summary>
        /// <returns></returns>
        IServiceResolver ManagedScope(object parameter);

        /// <summary>
        /// Creates a new IServiceResolver that should be scopped to a default transaction
        /// </summary>
        /// <returns></returns>
        IServiceResolver ManagedScope();
    }
}
