using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace d9.ucm;
// "tag" -> item.AllTags.Contains("tag")
// "youtube::tag" -> item.Sources.Named("youtube")?.HasTag("tag")
// "*::tag" -> item.Sources.All(x => x.HasTag("tag"))
// 
public class SearchToken
{
    private Func<Item, bool> _matchFunction;
    public SearchToken(string str)
    {
        string[] split = str.Split("::");
        _matchFunction = split.Length switch
        {
            1 => (item) => item.AllTags.Contains(str.TagNormalize()),
        };
    }
    public static implicit operator Func<Item, bool>(SearchToken st) => st._matchFunction;
    public static IEnumerable<SearchToken> Tokenize(string s) => s.Split(" ").Select(x => new SearchToken(x));
}
