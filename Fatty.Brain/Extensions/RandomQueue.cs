namespace Fatty.Brain.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public sealed class RandomQueue<T>
    {
        private Random random;
        private T[] source;
        private Queue<T> queue;

        public RandomQueue(Random random, T[] source)
        {
            this.queue = new Queue<T>();
            this.random = random;
            this.source = source;
        }

        private void FillQueue()
        {
            var copy = (T[])source.Clone();
            this.random.Shuffle(copy);
            foreach (var item in copy)
            {
                this.queue.Enqueue(item);
            }
        }

        public T Next()
        {
            if (this.queue.Count == 0)
            {
                this.FillQueue();
            }

            return this.queue.Dequeue();
        }
    }
}
