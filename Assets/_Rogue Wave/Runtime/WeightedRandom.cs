using System.Collections.Generic;
using UnityEngine;

namespace RogueWave
{
    /// <summary>
    /// Create an arbitrary collection of items with associated weights. When GetRandom() is called, 
    /// the items are returned with a probability based on their weight.
    /// </summary>
    /// <typeparam name="T">The type of entries allows in the collection</typeparam>
    public class WeightedRandom<T>
    {
        private struct Entry
        {
            public float weight;
            public T item;
        }

        private List<Entry> entries = new List<Entry>();
        private float accumulatedWeight;

        public int Count
        {
            get { return entries.Count; }
        }

        public void Add(T item, float weight)
        {
            accumulatedWeight += weight;
            entries.Add(new Entry { item = item, weight = weight });
        }

        public void Remove(T item)
        {
            Entry entry = entries.Find(e => e.item.Equals(item));
            if (entry.item != null)
            {
                accumulatedWeight -= entry.weight;
                entries.Remove(entry);
            }
        }

        public T GetRandom()
        {
            float rnd = Random.value * accumulatedWeight;
            float checkedWeight = 0;

            foreach (Entry entry in entries)
            {
                checkedWeight += entry.weight;
                if (checkedWeight >= rnd)
                {
                    return entry.item;
                }
            }
            return default(T); //should only happen when there are no entries
        }
    }
}
