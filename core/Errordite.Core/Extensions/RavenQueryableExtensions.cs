
using System;
using System.Linq;
using System.Linq.Expressions;
using Raven.Client.Linq;

namespace Errordite.Core.Extensions
{
    public static class RavenQueryableExtensions
    {
        public static IRavenQueryable<T> ConditionalWhere<T>(this IRavenQueryable<T> query, Expression<Func<T, bool>> predicate, Func<bool> condition)
        {
            if (condition())
                query = query.Where(predicate);

            return query;
        }

        public static IQueryable<T> ConditionalWhere<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate, Func<bool> condition)
        {
            if (condition())
                query = query.Where(predicate);

            return query;
        }
    }
}
