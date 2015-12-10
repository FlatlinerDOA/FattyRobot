using System;
using System.Collections;
using System.Collections.Generic;

namespace Fatty.Brain.Extensions
{
    internal sealed class RandomQueueEnumerator<T> : IEnumerator<T>
    {
        private RandomQueue<T> randomQueue;

        private T value;

        public RandomQueueEnumerator(RandomQueue<T> randomQueue)
        {
            this.randomQueue = randomQueue;
        }

        public T Current
        {
            get
            {
                return this.value;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return this.value;
            }
        }

        public void Dispose()
        {
            this.value = default(T);
        }

        public bool MoveNext()
        {
            this.value = this.randomQueue.Next();
            return true;
        }

        public void Reset()
        {
            this.randomQueue.Reset();
        }
    }
}