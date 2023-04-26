using System;
using System.Collections.Generic;
using System.Linq;

namespace RemarkableSync.document
{
    /**
     * Data structure representing CRDT sequence.
     */
    internal class CrdtSequenceItem<T>
    {
        public CrdtId item_id;
        public CrdtId left_id;
        public CrdtId right_id;
        public int deleted_length;
        public T value;
    }

    /***
     * Ordered CRDT Sequence container.
     * 
     * The Sequence contains `CrdtSequenceItem`s, each of which has an ID and
     * left/right IDs establishing a partial order.
     * 
     * Iterating through the `CrdtSequence` yields IDs following this order.
     * 
     */
    internal class CrdtSequence<T>
    {
        //List<CrdtSequenceItem<T>> _items = new List<CrdtSequenceItem<T>>();
        private Dictionary<CrdtId, CrdtSequenceItem<T>> _items = new Dictionary<CrdtId, CrdtSequenceItem<T>>();
        public CrdtSequence() { }

        public CrdtSequence(IEnumerable<CrdtSequenceItem<T>> items = null)
        {
            if (items == null)
            {
                items = new List<CrdtSequenceItem<T>>();
            }

            _items = items.ToDictionary(item => item.item_id, item => item);
        }

        public bool Equals(CrdtSequence<T> other)
        {
            if (other == null)
            {
                return false;
            }

            return _items.SequenceEqual(other._items);
        }

        public override bool Equals(object obj)
        {
            if (obj is CrdtSequence<T> sequence)
            {
                return Equals(sequence);
            }

            if (obj is IEnumerable<CrdtSequenceItem<T>> items)
            {
                return Equals(new CrdtSequence<T>(items));
            }

            return false;
        }

        public override int GetHashCode()
        {
            return _items.GetHashCode();
        }

        public override string ToString()
        {
            return $"CrdtSequence({_items.Values})";
        }

        public IEnumerable<CrdtId> GetSequence()
        {
            return ToposortItems(_items.Values);
        }

        public List<CrdtId> GetKeys()
        {
            return GetSequence().ToList();
        }

        public List<T> GetValues()
        {
            return GetKeys().Select(key => _items[key].value).ToList();
        }

        public IEnumerable<(CrdtId, T)> GetItems()
        {
            return GetKeys().Select(key => (key, _items[key].value));
        }

        public T GetItem(CrdtId key)
        {
            if (_items.ContainsKey(key))
            {
                return _items[key].value;
            }

            throw new KeyNotFoundException();
        }

        public List<CrdtSequenceItem<T>> GetSequenceItems()
        {
            return _items.Values.ToList();
        }

        public void Add(CrdtSequenceItem<T> item)
        {
            if (_items.ContainsKey(item.item_id))
            {
                throw new ArgumentException($"Already have item {item.item_id}");
            }

            _items[item.item_id] = item;
        }

        private IEnumerable<CrdtId> ToposortItems(IEnumerable<CrdtSequenceItem<T>> items)
        {
            CrdtId END_MARKER = new CrdtId(0, 0);
            CrdtId LEFT_MARKER = new CrdtId(0, "__start".GetHashCode());
            CrdtId RIGHT_MARKER = new CrdtId(0, "__end".GetHashCode());

            var item_dict = items.ToDictionary(item => item.item_id);
            if (!item_dict.Any())
            {
                yield break; //nothing to do
            }

            CrdtId GetSideId(CrdtSequenceItem<T> item, string side)
            {
                var side_id = (CrdtId)item.GetType().GetProperty($"{side}_id").GetValue(item);
                return side_id == END_MARKER ? (side == "left" ? LEFT_MARKER : RIGHT_MARKER) : side_id;
            }


            // build dictionary: key "comes after" values
            var data = new Dictionary<CrdtId, HashSet<CrdtId>>();
            foreach (CrdtSequenceItem<T> item in item_dict.Values)
            {
                var left_id = GetSideId(item, "left");
                var right_id = GetSideId(item, "right");
                if (!data.ContainsKey(item.item_id))
                {
                    data[item.item_id] = new HashSet<CrdtId>();
                }
                data[item.item_id].Add(left_id);
                if (!data.ContainsKey(right_id))
                {
                    data[right_id] = new HashSet<CrdtId>();
                }
                data[right_id].Add(item.item_id);
            }

            // fill in sources not explicitly included
            /*var sourcesNotInData = data.Values.SelectMany(deps => deps).ToHashSet() - data.Keys.ToHashSet();*/
            HashSet<CrdtId> sourcesNotInData = new HashSet<CrdtId>();
            foreach (var deps in data.Values)
            {
                foreach (var dep in deps)
                {
                    if (!data.ContainsKey(dep))
                    {
                        sourcesNotInData.Add(dep);
                    }
                }
            }
            foreach (var source in sourcesNotInData)
            {
                data.Add(source, new HashSet<CrdtId>());
            }

            while (true)
            {
                var next_items = new HashSet<CrdtId>(data.Where(kv => kv.Value.Count == 0).Select(kv => kv.Key));
                if (next_items.Count == 1 && next_items.Contains(new CrdtId(0, "__end".GetHashCode())))
                {
                    break;
                }
                if (next_items.Count == 0)
                {
                    throw new ArgumentException("cyclic dependency");
                }
                foreach (var item_id in next_items.OrderBy(id => id))
                {
                    if (item_dict.ContainsKey(item_id))
                    {
                        yield return item_id;
                    }
                }
                data = data.Where(kv => !next_items.Contains(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);
                foreach (var deps in data.Values)
                {
                    deps.ExceptWith(next_items);
                }
            }
        }

    }
    public class CrdtId
    {
        public int part1;
        public int part2;
        public CrdtId(int part1, int part2)
        {
            this.part1 = part1;
            this.part2 = part2;
        }
    }

    /**
     * Container for a last-write-wins value.
     */
    public class LwwValue<T>
    {
        public CrdtId timestamp;
        public T value;

        public LwwValue(CrdtId timestamp, T value)
        {
            this.timestamp = timestamp;
            this.value = value;
        }
    }
}
