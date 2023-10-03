using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace d9.ucm;
public abstract class UrlType 
{ 
    public string Value { get; private set; }
    public UrlType(string s)
    {
        Value = s;
    }
    public override string ToString() => Value;
}
public class PageUrl : UrlType
{
    public PageUrl(string s) : base(s) { }
}
public class ApiUrl : UrlType
{
    public ApiUrl(string s) : base(s) { }
}
