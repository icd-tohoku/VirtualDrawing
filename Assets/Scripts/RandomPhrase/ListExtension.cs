using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace TypingPhraseDisplaySample
{
    public static class ListExtension
    {
        public static IEnumerable<T> GetRandom<T>(this IList<T> list, int count) =>
            Enumerable.Range(0, list.Count)
                .OrderBy(_ => Guid.NewGuid())
                .Take(count)
                .Select(i => list[i]);
    }
}