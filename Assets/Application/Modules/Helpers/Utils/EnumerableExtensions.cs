using System;
using System.Collections.Generic;
using System.Linq;
using Random = System.Random;

public static class EnumerableExtensions
{
    public static Random randomizer = new Random();
    public static T GetRandomElement<T>(this T[] array)
    {
        if (array == null || array.Length == 0)
        {
            throw new ArgumentException("Array must not be null or empty");
        }
        
        int randomIndex = randomizer.Next(array.Length);
        return array[randomIndex];
    }

    public static T GetRandomElement<T>(this List<T> list)
    {
        if (list == null || list.Count == 0)
        {
            throw new ArgumentException("List must not be null or empty");
        }
        int randomIndex = randomizer.Next(list.Count);
        return list[randomIndex];
    }
    public static T GetRandomElementWithSeed<T>(this List<T> list, int seed)
    {
        if (list == null || list.Count == 0)
        {
            throw new ArgumentException("List must not be null or empty");
        }
        System.Random randomizer = new System.Random(seed);

        int randomIndex = randomizer.Next(list.Count);
        return list[randomIndex];
    }
    public static KeyValuePair<TKey, TValue> GetRandomElement<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
    {
        if (dictionary == null || dictionary.Count == 0)
        {
            throw new ArgumentException("Dictionary must not be null or empty");
        }

        int randomIndex = randomizer.Next(dictionary.Count);
        return dictionary.ElementAt(randomIndex);
    }
}