namespace Fatty.Brain
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public sealed class Intent : IDictionary<string, string>
    {
        private readonly Dictionary<string, string> args;

        public Intent(string name)
        {
            this.Name = name;
            this.args = new Dictionary<string, string>();
            this.Priority = 2;
            this.Probability = 1d;
            this.Weight = 0.5d;
        }

        public Intent(string name, double probability, int priority)
        {
            this.Name = name;
            this.args = new Dictionary<string, string>();
            this.Priority = priority;
            this.Probability = probability;
            this.Weight = this.Probability * (1d / this.Priority);
        }

        public Intent(string name, double probability, int priority, Dictionary<string, string> args)
        {
            this.Name = name;
            this.args = args;
            this.Probability = probability;
            this.Priority = priority;
            this.Weight = this.Probability * (1d / this.Priority);
        }

        public string Name
        {
            get; private set;
        }

        public double Probability
        {
            get; private set;
        }

        public ICollection<string> Keys
        {
            get
            {
                return this.args.Keys;
            }
        }

        public ICollection<string> Values
        {
            get
            {
                return this.args.Values;
            }
        }

        public int Count
        {
            get
            {
                return this.args.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public int Priority
        {
            get;
            private set;
        }

        public double Weight
        {
            get;
            private set;
        }

        public string this[string key]
        {
            get
            {
                return this.args[key];
            }

            set
            {
                this.args[key] = value;
            }
        }

        public void Add(string key, string value)
        {
            this.args.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return this.args.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return this.args.Remove(key);
        }

        public bool TryGetValue(string key, out string value)
        {
            return this.args.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<string, string> item)
        {
            this.args.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            this.args.Clear();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            return this.args.ContainsKey(item.Key);
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            return this.Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return this.args.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.args.GetEnumerator();
        }
    }
}
