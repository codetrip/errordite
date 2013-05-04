using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;

namespace Errordite.Core.IoC
{
    /// <summary>
    /// From http://hammett.castleproject.org/?p=257
    /// 
    /// Means that if a dependency (ctor param or property) is an array of interfaces
    /// it will be resolved to all registered implementations of that service in Windsor.
    /// 
    /// The initial reason for using this was for IAuditAugmenters.
    /// </summary>
    public class ArrayResolver : ISubDependencyResolver
    {
        private readonly IKernel _kernel;

        public ArrayResolver(IKernel kernel)
        {
            _kernel = kernel;
        }

        public object Resolve(CreationContext context, ISubDependencyResolver parentResolver, ComponentModel model, DependencyModel dependency)
        {
            return _kernel.ResolveAll(dependency.TargetType.GetElementType(), null);
        }

        public bool CanResolve(CreationContext context, ISubDependencyResolver parentResolver, ComponentModel model, DependencyModel dependency)
        {
            return dependency.TargetType != null && dependency.TargetType.IsArray && dependency.TargetType.GetElementType().IsInterface;
        }
    }
}