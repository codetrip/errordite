using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace CodeTrip.Core.Extensions
{
    public static class EnumerableExtensions
    {
        public static T FirstOrDefaultFromMany<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> childrenSelector, Func<T, bool> condition) where T : class
        {
            if (source == null)
                return null;

            source = source.ToList();

            // return result if found and stop traversing hierarchy
            var attempt = source.FirstOrDefault(condition);

            if (attempt != null) 
                return attempt;

            // recursively call this function on lower levels of the
            // hierarchy until a match is found or the hierarchy is exhausted
            var items = source.SelectMany(childrenSelector);

            if(items.Any())
                return source.SelectMany(childrenSelector).FirstOrDefaultFromMany(childrenSelector, condition);

            return null;
        }

        public static string ToRavenQuery<T>(this IEnumerable<T> source, string fieldName)
        {
            return "({0})".FormatWith(source.Aggregate(string.Empty, (current, id) => current + ("{0}:\"{1}\" OR ".FormatWith(fieldName, id))).TrimEnd(new[] { ' ', 'O', 'R' }));
        }

        public static string StringConcat<T>(this IEnumerable<T> source, Func<T, string> toString)
        {
            return source.Aggregate(new StringBuilder(), (acc, member) => acc.Append(toString(member)), x => x.ToString());
        }

        public static string StringConcat(this IEnumerable<string> source, string delimiter)
        {
            return StringConcat(source, x => x + delimiter);
        }

        public static IEnumerable<T> OrIfNoneThen<T>(this IEnumerable<T> enumerable, params T[] ifNone)
        {
            return enumerable.Any() ? enumerable : ifNone;
        }

        /// <summary>
        /// Method to partition an IEnumerable into chunks, courtesy of Jon Skeet
        /// http://stackoverflow.com/questions/438188/split-a-collection-into-n-parts-with-linq/438208#438208
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> source, int size)
        {
            T[] array = null;
            int count = 0;
            foreach (T item in source)
            {
                if (array == null)
                {
                    array = new T[size];
                }
                array[count] = item;
                count++;
                if (count == size)
                {
                    yield return new ReadOnlyCollection<T>(array);
                    array = null;
                    count = 0;
                }
            }
            if (array != null)
            {
                Array.Resize(ref array, count);
                yield return new ReadOnlyCollection<T>(array);
            }
        }
    }
}