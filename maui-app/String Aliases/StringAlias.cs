using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace d9.ucm;
public abstract class StringAlias 
{ 
    public string Value { get; private set; }
    public StringAlias(string s)
    {
        Value = s;
    }
    public override string ToString() => Value;
}