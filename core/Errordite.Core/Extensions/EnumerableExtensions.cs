using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Errordite.Core.Extensions
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

        /// <summary>
        /// Turn some sequence of stuff into a string.
        /// </summary>
        /// <param name="toString">How we turn some arbitrary element into a string.</param>
        /// <param name="toStringLast">How we turn the last element into a string.  If not specified uses toString</param>
        /// <param name="toStringFirst">How we turn the first element into a string.  If not specified uses toString</param>
        /// <returns></returns>
        public static string StringConcat<T>(this IEnumerable<T> source, Func<T, string> toString, Func<T, string> toStringLast = null, Func<T, string> toStringFirst = null)
        {
            toStringLast = toStringLast ?? toString;
            toStringFirst = toStringFirst ?? toString;

            var lsource = source.ToList();
            int total = lsource.Count;

            int ii = 0;
            return lsource.Aggregate(
                new StringBuilder(),
                (acc, member) =>
                {
                    ii++;
                    var ret = acc.Append(ii == total
                                             ? toStringLast(member)
                                             : ii == 1 ? toStringFirst(member) : toString(member));
                    return ret;
                },
                x => x.ToString());
        }

        /// <summary>
        /// Concatenates some strings.
        /// </summary>
        /// <param name="delimiter">What to put between the strings.</param>
        /// <param name="trimEnd">Do we want to not put the delimiter at the end?  Defaults to false (i.e. leave trailing delimiter)</param>
        /// <param name="lastDelimiter">Do we want a different last delimiter.  This is with respect to trim end so will appear before last element
        /// if trimEnd; at the end otherwise.</param>
        /// <returns></returns>
        public static string StringConcat(this IEnumerable<string> source, string delimiter, bool trimEnd = false, string lastDelimiter = null)
        {
            //just deal with the special case of only one item with end trimmed as it was getting too hairy trying to 
            //work it out otherwise!
            if (source.Count() == 1 && trimEnd)
                return source.First();

            if (lastDelimiter == null)
            {
                return StringConcat(
                    source,
                    x => x + delimiter,
                    trimEnd ? x => x : (Func<string, string>)null);
            }
            else
            {
                if (trimEnd)
                {
                    return StringConcat(
                        source,
                        x => delimiter + x,
                        toStringLast: x => lastDelimiter + x,
                        toStringFirst: x => x
                        );
                }
                else
                {
                    return StringConcat(
                        source,
                        x => x + delimiter,
                        toStringLast: x => x + lastDelimiter);
                }
            }

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