
using Castle.DynamicProxy;
using Errordite.Core.Caching.Interfaces;

namespace Errordite.Core.Caching.Interceptors
{
    public interface ICacheInvalidationInterceptor : IInterceptor
    {}

    public class CacheInvalidationInterceptor : ComponentBase, ICacheInvalidationInterceptor
    {
        public const string IoCName = "DefaultCacheInvalidator";
        private readonly ICacheInvalidator _cacheInvalidator;

        public CacheInvalidationInterceptor(ICacheInvalidator cacheInvalidator)
        {
            _cacheInvalidator = cacheInvalidator;
        }

        /// <summary>
        /// Intercepts a method and after invocation checks to see if the request implements ICacheable
        /// if so, the ICacheItemInvalidator is invoked to remove the item from the cache
        /// </summary>
        public virtual void Intercept(IInvocation invocation)
        {
            ArgumentValidation.ComponentNotNull(_cacheInvalidator);

            invocation.Proceed();

            _cacheInvalidator.Invalidate(invocation.ReturnValue as ICacheInvalidation);
        }
    }
}
