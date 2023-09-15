using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace d9.ucm;
public static class IdManager
{
    public static ItemId Id { get; private set; } = 0;
    /// <summary>
    /// Updates the manager so that the current id is always greater than the highest registered id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static ItemId Register(ItemId? id = null)
    {
        id ??= Id;
        if (id >= Id)
            Id = id.Value + 1;
        return id.Value;
    }
}