using System;
using System.Collections.Generic;

namespace Axis.Luna
{
    public interface IResolutionScopeProvider: IDisposable
    {
        /// <summary>
        /// Creates a new IServiceResolver that should be scopped to a "context" possibly specified via the parameter
        /// </summary>
        /// <returns></returns>
        IServiceResolver ResolutionScope(object parameter);

        /// <summary>
        /// Creates a new IServiceResolver that should be scopped to a default "context"
        /// </summary>
        /// <returns></returns>
        IServiceResolver ResolutionScope();
    }

    public interface IServiceResolver : IDisposable
    {
        object Resolve(Type serviceType, params object[] args);

        Service Resolve<Service>(params object[] args);

        IEnumerable<object> ResolveAll(Type serviceType, params object[] args);

        IEnumerable<Service> ResolveAll<Service>(params object[] args);
    }
}
