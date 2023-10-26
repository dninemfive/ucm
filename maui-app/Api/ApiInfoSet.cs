using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace d9.ucm;
public class ApiInfoSet : IEnumerable<KeyValuePair<string, string?>>
{
    private Dictionary<string, string?> _dict = new();
    public ApiInfoSet(Dictionary<string, string?> dict)
    {
        _dict = dict;
    }
    public string? this[string key] => _dict[key];

    public IEnumerator<KeyValuePair<string, string?>> GetEnumerator()
        => _dict.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => _dict.GetEnumerator();
}
