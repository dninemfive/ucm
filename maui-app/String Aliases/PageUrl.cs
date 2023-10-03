using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace d9.ucm;
/// <summary>
/// Represents a non-API webpage, i.e. either a raw URL or a canonical one
/// </summary>
public class PageUrl : StringAlias
{
    public PageUrl(string s) : base(s) { }
}