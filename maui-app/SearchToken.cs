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
}
