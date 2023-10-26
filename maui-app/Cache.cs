using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace d9.ucm;
internal class Cache<K, V>
    where K : notnull
{
    private Dictionary<K, V?> _dict = new();
    private Func<K, V?> _default;
    public int Hits { get; private set; } = 0;
    public int Misses { get; private set; } = 0;
    public float HitRate => Hits / (float)(Hits + Misses);
    public Cache(Func<K, V?> defaultFn) 
    {
        _default = defaultFn;
    }
    public V? this[K key]
    {
        get
        {
            if (_dict.TryGetValue(key, out V? value))
            {
                Hits++;
                return value;
            }
            Misses++;
            _dict[key] = _default(key);
            return _dict[key];
        }
    }
}
