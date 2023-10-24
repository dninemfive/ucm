using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace d9.ucm;
internal class Scraper : ApiDef
{
    public override Task<string?> GetFileUrlAsync(UrlSet urlSet)
    {
        // request + cache html
        // find/construct url
        // return it
        // possibly start a timeout to avoid getting rate-limited or blocked
        throw new NotImplementedException();
    }
    public override Task<IEnumerable<string>?> GetTagsAsync(UrlSet urlSet)
    {
        // look at cached html
        // find tag info idk
        return base.GetTagsAsync(urlSet);
    }
}
