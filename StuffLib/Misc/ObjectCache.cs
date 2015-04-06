using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class ObjectCache
    {
        private Dictionary<int, object> Cache;
        private Dictionary<int, DateTime> Times;
        public int MaxCount;
        public Func<int, object> Getter;

        public ObjectCache(Func<int, object> getter, int maxCount = 10)
        {
            MaxCount = maxCount;
            Cache = new Dictionary<int, object>();
            Times = new Dictionary<int, DateTime>();
            Getter = getter;
        }

        public void Clear()
        {
            Cache.Clear();
            Times.Clear();
        }

        public bool ContainsId(int id)
        {
            return Cache.ContainsKey(id);
        }

        public object this[int id]
        {
            get
            {
                if (Cache.ContainsKey(id))
                {
                    Times[id] = DateTime.Now;
                    return Cache[id];
                }

                if (Getter == null) return null;
                var o = Getter(id);

                Cache.Add(id, o);
                Times.Add(id, DateTime.Now);

                if (Cache.Count > MaxCount)
                {
                    int rem = Times.OrderBy(x => x.Value).First().Key;
                    Cache.Remove(rem);
                    Times.Remove(rem);
                }

                return o;
            }
            set
            {
                if (Cache.ContainsKey(id))
                {
                    Cache[id] = value;
                    Times[id] = DateTime.Now;
                }
                else
                {
                    Cache.Add(id, value);
                    Times.Add(id, DateTime.Now);

                    if (Cache.Count > MaxCount)
                    {
                        int rem = Times.OrderBy(x => x.Value).First().Key;
                        Cache.Remove(rem);
                        Times.Remove(rem);
                    }
                }
            }
        }
    }
}
