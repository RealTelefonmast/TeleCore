﻿using System.Collections.Generic;
using Verse;

namespace TeleCore
{
    public static class TGenCollections
    {
        public static void Populate<T>(this T[] array, IEnumerable<T> values)
        {
            int i = 0;
            foreach (var value in values)
            {
                array[i] = value;
                i++;
            }
        }

        public static Dictionary<T, T2> Copy<T, T2>(this Dictionary<T, T2> old)
        {
            Dictionary<T, T2> newDict = new Dictionary<T, T2>();
            foreach (T o in old.Keys)
                newDict.Add(o, old.TryGetValue(o));
            return newDict;
        }

        public static void Move<T>(this List<T> list, int oldIndex, int newIndex)
        {
            if (oldIndex < 0 || oldIndex >= list.Count)
            {
                TLog.Warning($"Tried to move item from Index: {oldIndex} in collection of {list.Count}");
                return;
            }
            var item = list[oldIndex];
            list.RemoveAt(oldIndex);
            var adjIndex = newIndex;
            list.Insert(adjIndex, item);
        }
    }
}
