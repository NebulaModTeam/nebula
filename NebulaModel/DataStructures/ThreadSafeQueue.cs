using System.Collections.Generic;

namespace NebulaModel.DataStructures
{
    public class ThreadSafeQueue<T>
    {
        private readonly Queue<T> queue;
        private readonly object lockObj = new object();

        public ThreadSafeQueue()
        {
            queue = new Queue<T>();
        }

        public int Count
        {
            get
            {
                lock (lockObj)
                {
                    return queue.Count;
                }
            }
        }

        public void Clear()
        {
            lock (lockObj)
            {
                queue.Clear();
            }
        }

        public bool Contains(T item)
        {
            lock (lockObj)
            {
                return queue.Contains(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (lockObj)
            {
                queue.CopyTo(array, arrayIndex);
            }
        }

        public T Dequeue()
        {
            lock (lockObj)
            {
                return queue.Dequeue();
            }
        }

        public void Enqueue(T item)
        {
            lock (lockObj)
            {
                queue.Enqueue(item);
            }
        }

        public T Peek()
        {
            lock (lockObj)
            {
                return queue.Peek();
            }
        }

        public T[] ToArray()
        {
            lock (lockObj)
            {
                return queue.ToArray();
            }
        }

        public void TrimExcess()
        {
            lock (lockObj)
            {
                queue.TrimExcess();
            }
        }
    }
}
