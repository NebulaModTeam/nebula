using System;
using System.Collections;
using System.Collections.Generic;

namespace NebulaModel.DataStructures
{
    public class ThreadSafeDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly IDictionary<TKey, TValue> dictionary;
        private readonly object lockObj = new object();

        public ThreadSafeDictionary()
        {
            dictionary = new Dictionary<TKey, TValue>();
        }

        public TValue this[TKey key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ICollection<TKey> Keys
        {
            get
            {
                lock (lockObj)
                {
                    return new List<TKey>(dictionary.Keys);
                }
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                lock (lockObj)
                {
                    return new List<TValue>(dictionary.Values);
                }
            }
        }

        public int Count
        {
            get
            {
                lock (lockObj)
                {
                    return dictionary.Count;
                }
            }
        }

        public bool IsReadOnly { get; } = false;

        public void Add(TKey key, TValue value)
        {
            lock (lockObj)
            {
                dictionary.Add(key, value);
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            lock (lockObj)
            {
                dictionary.Add(item);
            }
        }

        public void Clear()
        {
            lock (lockObj)
            {
                dictionary.Clear();
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            lock (lockObj)
            {
                return dictionary.Contains(item);
            }
        }

        public bool ContainsKey(TKey key)
        {
            lock (lockObj)
            {
                return dictionary.ContainsKey(key);
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            lock (lockObj)
            {
                dictionary.CopyTo(array, arrayIndex);
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            lock (lockObj)
            {
                return new Dictionary<TKey, TValue>(dictionary).GetEnumerator();
            }
        }

        public bool Remove(TKey key)
        {
            lock (lockObj)
            {
                return dictionary.Remove(key);
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            lock (lockObj)
            {
                return dictionary.Remove(item);
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (lockObj)
            {
                return dictionary.TryGetValue(key, out value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
