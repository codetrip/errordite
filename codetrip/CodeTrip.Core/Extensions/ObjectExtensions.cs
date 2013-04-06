using System;
using System.Linq;

namespace CodeTrip.Core.Extensions
{
    public static class ObjectExtensions
    {
        public static bool IsIn<T>(this T obj, params T[] candidates)
        {
            return candidates.Any(c => c == null ? obj == null : c.Equals(obj));
        }     

        public static TOut IfPoss<T, TOut>(this T obj, Func<T, TOut> getter, TOut valueIfNotPoss = default(TOut)) 
            where T : class 
        {
            return obj.Cond(t => t == null, t => valueIfNotPoss, getter);
        }

        public static TOut Cond<T, TOut>(this T obj, Func<T, bool> test, Func<T, TOut> resultIf, Func<T, TOut> resultElse)
        {
            return test(obj) ? resultIf(obj) : resultElse(obj);
        }
    }
}