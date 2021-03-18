using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace PaSharperExtension.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// It's like Zip but for single type and without skipping unpaired values
        /// </summary>
        public static IEnumerable<T> MergeZip<T>(
            [NotNull] this IEnumerable<T> first,
            [NotNull] IEnumerable<T> second,
            [NotNull] Func<T, T, T> resultSelector)
        {
            _ = first ?? throw new ArgumentNullException(nameof(first));
            _ = second ?? throw new ArgumentNullException(nameof(second));
            _ = resultSelector ?? throw new ArgumentNullException(nameof(resultSelector));

            using var en1 = first.GetEnumerator();
            using var en2 = second.GetEnumerator();

            bool hasEn1Next;
            bool hasEn2Next;

            while (true)
            {
                hasEn1Next = en1.MoveNext();
                hasEn2Next = en2.MoveNext();

                if (hasEn1Next && hasEn2Next)
                {
                    yield return resultSelector(en1.Current, en2.Current);
                }
                else
                {
                    break;
                }
            }

            if (hasEn1Next)
            {
                yield return en1.Current;
                while (en1.MoveNext())
                {
                    yield return en1.Current;
                }
            }

            if (hasEn2Next)
            {
                yield return en2.Current;
                while (en2.MoveNext())
                {
                    yield return en2.Current;
                }
            }
        }
    }
}
