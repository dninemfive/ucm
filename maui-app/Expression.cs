using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace d9.ucm;
public interface IExpression
{
    public bool Evaluate();
}
public class GenericExpression<T> : IExpression
{
    public T? Value { get; private set; }
    public Func<T?, bool> Expression { get; private set; }
    public bool Evaluate() => Expression(Value);
    public GenericExpression(T? value, Func<T?, bool> expression)
    {
        Value = value;
        Expression = expression;
    }
}
public abstract class MetaExpression : IExpression 
{
    private readonly IExpression[] _children;
    public IEnumerable<IExpression> Children => _children;
    protected bool _evaluate(bool defaultValue)
    {
        foreach(IExpression child in Children)
        {
            if (child.Evaluate() == defaultValue)
                return defaultValue;
        }
        return !defaultValue;
    }
    public abstract bool Evaluate();
    public MetaExpression(params IExpression[] children)
    {
        _children = children;
    }
}
public class OrExpression : MetaExpression
{
    public override bool Evaluate() => _evaluate(true);
}
public class AndExpression : MetaExpression
{
    public override bool Evaluate() => _evaluate(false);
}