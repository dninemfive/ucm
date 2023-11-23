using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace d9.ucm;
public interface IItemViewable
{
#pragma warning disable CS1998 // no synchronous calls: my C# in christ YOU said i had to have a method body on this interface method
    public async Task<IEnumerable<ItemSource>> GetItemSourcesAsync()
        => throw new NotImplementedException();
#pragma warning restore CS1998
    public View View { get; }
    public Label InfoLabel { get; }
}
