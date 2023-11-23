using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace d9.ucm;
public interface IItemFilter
{
    public View View { get; }
    public static string Name { get; }
    public Expression<Func<Item, bool>> Expression { get; }
}
public class ItemFilter_Tag : IItemFilter
{
    public static string Name => "Has Tag";
    public View View
    {
        get
        {
            HorizontalStackLayout result = new();
            // result.add(new entry which lowercases inputs)
            // ... and which, when filled, has the option to say "or"?
            // or maybe just Has Tag(s) and a single entry
            return result;
        }
    }
    public string Tag { get; private set; }
    public Expression<Func<Item, bool>> Expression
        => throw new NotImplementedException();
}
