using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace d9.ucm;
public delegate bool Expression<T>(T value, params Expression<T>[] children);
public static class Expressions
{
    public static bool Evaluate<T>(this Expression<T> root, T value, params Expression<T>[] children)
        => root(value, children);
    public static bool Or<T>(T value, params Expression<T>[] children)
    {
        foreach(Expression<T> child in children)
        {
            if(child(value)) return true;
        }
        return false;
    }
    public static bool And<T>(T value, params Expression<T>[] children)
    {
        foreach (Expression<T> child in children)
        {
            if (!child(value))
                return false;
        }
        return true;
    }
}