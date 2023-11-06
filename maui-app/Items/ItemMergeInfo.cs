using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace d9.ucm;
public class ItemMergeInfo
{
    [JsonInclude]
    public ItemId ResultId { get; private set; }
    [JsonInclude]
    public List<ItemId> ParentIds { get; private set; }
    [JsonConstructor]
    public ItemMergeInfo(ItemId resultId, List<ItemId> parentIds)
    {
        ResultId = resultId;
        ParentIds = parentIds;
    }
}
