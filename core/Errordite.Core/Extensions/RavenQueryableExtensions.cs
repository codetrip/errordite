
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Raven.Client;
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

        public static IList<T> GetAllItemsAsList<T>(this IRavenQueryable<T> query, int pageSize)
        {
            RavenQueryStatistics stats;

            var items = query.Statistics(out stats)
                .Skip(0)
                .Take(pageSize)
                .As<T>()
                .ToList();

            if (stats.TotalResults > pageSize)
            {
                int pageNumber = stats.TotalResults / pageSize;

                for (int i = 1; i < pageNumber; i++)
                {
                    items.AddRange(query
                        .Skip(i * pageSize)
                        .Take(pageSize)
                        .As<T>());
                }
            }

            return items.ToList();
        }
    }
}
