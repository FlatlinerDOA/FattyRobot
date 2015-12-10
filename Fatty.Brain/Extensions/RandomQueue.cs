namespace Fatty.Brain.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public sealed class RandomQueue<T> : IEnumerable<T>
    {
        private readonly Random random;
        private readonly List<T> source;
        private readonly Queue<T> queue;

        public RandomQueue()
        {
            this.queue = new Queue<T>();
            this.random = new Random();
            this.source = new List<T>();
        }

        public RandomQueue(Random random, IEnumerable<T> source)
        {
            this.queue = new Queue<T>();
            this.random = random;
            this.source = source.ToList();
        }

        private void FillQueue()
        {
            var copy = source.ToList();
            this.random.Shuffle(copy);
            foreach (var item in copy)
            {
                this.queue.Enqueue(item);
            }
        }

        internal void Reset()
        {
            this.queue.Clear();
            this.FillQueue();
        }

        public void Add(T value)
        {
            this.source.Add(value);
        }

        public T Next()
        {
            if (this.queue.Count == 0)
            {
                this.FillQueue();
            }

            return this.queue.Dequeue();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new RandomQueueEnumerator<T>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
