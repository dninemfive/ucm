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
    private readonly Func<Item, Task<bool>> _matchFunction;
    public SearchToken(string str)
    {
        string[] split = str.Split("::");
        _matchFunction = split.Length switch
        {
            1 => async (item) => (await item.GetAllTagsAsync()).Contains(str.TagNormalize()),
        };
    }
    public static implicit operator Func<Item, Task<bool>>(SearchToken st) => st._matchFunction;
    public static IEnumerable<SearchToken> Tokenize(string s) => s.Split(" ").Select(x => new SearchToken(x));
}
