using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using d9.utl;

namespace d9.ucm;
public abstract class ApiDef
{
#pragma warning disable CS1998 // "lacks await": intentionally not implemented
    public virtual async Task<string?> GetFileUrlAsync(TransformedUrl summary)
        => throw new NotImplementedException();
    public virtual async Task<IEnumerable<string>?> GetTagsAsync(UrlSet urlSet)
        => throw new NotImplementedException();
#pragma warning restore CS1998
}