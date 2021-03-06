﻿namespace Fatty.Brain
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using Extensions;
    public sealed class Intent : IDictionary<string, string>
    {
        private readonly Dictionary<string, string> args;

        public Intent(string name)
        {
            this.Name = name;
            this.args = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            this.Priority = 2;
            this.Probability = 1d;
            this.Weight = 0.5d;
        }

        public Intent(string name, IDictionary<string, string> keys)
        {
            this.Name = name;
            this.args = new Dictionary<string, string>(keys, StringComparer.OrdinalIgnoreCase);
            this.Priority = 2;
            this.Probability = 1d;
            this.Weight = 0.5d;
        }

        public static Intent Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                // No intent yet
                return null;
            }

            text = text.TrimEnd('\r', '\n', ';', '~');
            var intent = new Intent(text.SubstringToFirst(" "));
            if (text.IndexOf(' ') != -1)
            {
                var args = text.SubstringFromFirst(" ").Split(';');
                if (args.Length > 0)
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        var key = args[i].SubstringToFirst("=");
                        var value = args[i].SubstringFromFirst("=");
                        intent[key] = value;
                    }
                }
            }

            return intent;
        }

        public Intent(string name, double probability, int priority)
        {
            this.Name = name;
            this.args = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            this.Priority = priority;
            this.Probability = probability;
            this.Weight = this.Probability * (1d / this.Priority);
        }

        public bool IsNamed(string name)
        {
            return string.Equals(this.Name, name, StringComparison.OrdinalIgnoreCase);
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
                if (this.args.ContainsKey(key))
                {
                    return this.args[key];
                }

                return null;
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

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(this.Name);

            if (this.Count == 0)
            {
                return sb.ToString();
            }

            sb.Append(" ");
            foreach (var kv in this)
            {
                sb.Append(kv.Key).Append("=").Append(kv.Value).Append(";");
            }

            return sb.ToString();
        }
    }
}
