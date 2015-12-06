namespace Fatty.Brain.Extensions
{
    using System;
    using System.Collections.Generic;

    public static class RandomExtensions
    {
        public static T Pick<T>(this Random random, IReadOnlyList<T> items)
        {
            if (items.Count == 0)
            {
                return default(T);
            }

            return items[random.Next(0, items.Count)];
        }

        public static void Shuffle<T>(this Random random, T[] array)
        {
            for (int i = array.Length; i > 1; i--)
            {
                // Pick random element to swap.
                int j = random.Next(i); // 0 <= j <= i-1
                                        // Swap.
                T tmp = array[j];
                array[j] = array[i - 1];
                array[i - 1] = tmp;
            }
        }
    }
}
