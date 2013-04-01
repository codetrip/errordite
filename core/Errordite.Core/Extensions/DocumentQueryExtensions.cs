using System;
using Raven.Client;

namespace Errordite.Core.Extensions
{
    public static class DocumentQueryExtensions
    {
        public static IDocumentQuery<T> ConditionalWhere<T>(this IDocumentQuery<T> query, string whereClause, bool condition)
        {
            if (condition)
                return query.Where(whereClause);

            return query;
        }

        public static IDocumentQuery<T> ConditionalSort<T>(this IDocumentQuery<T> query, string fieldName, bool descending, bool condition)
        {
            if (condition)
                return query.AddOrder(fieldName, descending);

            return query;
        }

        public static IDocumentQuery<T> ConditionalWaitForNonStaleResults<T>(this IDocumentQuery<T> query, bool condition)
        {
            if (condition)
                return query.WaitForNonStaleResults();

            return query;
        }

        public static IDocumentQuery<T> ConditionalWaitForNonStaleResults<T>(this IDocumentQuery<T> query, bool condition, TimeSpan waitTimeout)
        {
            if (condition)
                return query.WaitForNonStaleResults(waitTimeout);

            return query;
        }

        public static IDocumentQuery<T> ConditionalWaitForNonStaleResultsAsOfNow<T>(this IDocumentQuery<T> query, bool condition)
        {
            if (condition)
                return query.WaitForNonStaleResultsAsOfNow();

            return query;
        }

        public static IDocumentQuery<T> ConditionalWaitForNonStaleResultsAsOfLastWrite<T>(this IDocumentQuery<T> query, bool condition)
        {
            if (condition)
                return query.WaitForNonStaleResultsAsOfLastWrite();

            return query;
        }

        public static IDocumentQuery<T> ConditionalWaitForNonStaleResultsAsOfLastWrite<T>(this IDocumentQuery<T> query, bool condition, TimeSpan waitTimeout)
        {
            if (condition)
                return query.WaitForNonStaleResultsAsOfNow(waitTimeout);

            return query;
        }

        public static IDocumentQuery<T> ConditionalWaitForNonStaleResultsAsOfNow<T>(this IDocumentQuery<T> query, bool condition, TimeSpan waitTimeout)
        {
            if (condition)
                return query.WaitForNonStaleResultsAsOfNow(waitTimeout);

            return query;
        }

        public static IDocumentQuery<T> ConditionalWaitForNonStaleResultsAsOf<T>(this IDocumentQuery<T> query, bool condition, DateTime cutOff)
        {
            if (condition)
                return query.WaitForNonStaleResultsAsOf(cutOff);

            return query;
        }

        public static IDocumentQuery<T> ConditionalWaitForNonStaleResultsAsOf<T>(this IDocumentQuery<T> query, bool condition, DateTime cutOff, TimeSpan waitTimeout)
        {
            if (condition)
                return query.WaitForNonStaleResultsAsOf(cutOff, waitTimeout);

            return query;
        }
    }
}