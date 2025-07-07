// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    public static class ArrayHelper
    {
        /// <summary>
        /// Loại bỏ số phần tử trong mảng
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="numberOfElemment"></param>
        /// <returns></returns>
        public static IList<T> RemoveFirstTwoElements<T>(IList<T> list, int numberOfElemment)
        {
            if (list.Count >= numberOfElemment)
            {
                return list.Skip(numberOfElemment).ToList();
            }
            else
            {
                // Danh sách rỗng, trả về danh sách rỗng
                return new List<T>();
            }
        }

        public static int? IndexOfSubSet<T>(this IEnumerable<T> items, T item, Predicate<T> isBelongSubSet)
        {
            if (items == null || !items.Any() || item == null || isBelongSubSet == null)
            {
                return null;
            }

            if (!isBelongSubSet.Invoke(item))
            {
                return null;
            }

            int subSetCount = 0;
            for (int i = 0; i < items.Count(); i++)
            {
                if (isBelongSubSet.Invoke(items.ElementAt(i)))
                {
                    subSetCount++;
                    if (items.ElementAt(i).Equals(item))
                    {
                        return subSetCount - 1;
                    }
                }
            }

            return null;
        }

        public static void ForEachWithPrevious<T>(this IEnumerable<T> source, Action<T?, T> action)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using var enumerator = source.GetEnumerator();
            bool hasPrevious = false;
            T? previous = default;

            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                action(hasPrevious ? previous : default, current);
                previous = current;
                hasPrevious = true;
            }
        }
    }
}
