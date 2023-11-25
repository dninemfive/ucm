using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace d9.ucm;
public interface IItemViewable
{
    public Task<IEnumerable<ItemSource>> GetItemSourcesAsync();
    public View View { get; }
    public Label InfoLabel { get; }
}
