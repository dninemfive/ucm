using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace d9.ucm;
public interface IExpression
{
    public bool Evaluate(object? arg);
}
public class GenericExpression<T> : IExpression
    where T : class
{
    public Func<T?, bool> Expression { get; private set; }
    public bool Evaluate(object? arg) => Expression(arg as T);

    public GenericExpression(Func<T?, bool> expression)
    {
        Expression = expression;
    }
    public static implicit operator GenericExpression<T>(Func<T?, bool> func) => new(func);
}
public abstract class MetaExpression : IExpression 
{
    private readonly IExpression[] _children;
    public IEnumerable<IExpression> Children => _children;
    protected bool _evaluate(object? arg, bool defaultValue)
    {
        foreach(IExpression child in Children)
        {
            if (child.Evaluate(arg) == defaultValue)
                return defaultValue;
        }
        return !defaultValue;
    }
    public abstract bool Evaluate(object? arg);
    public MetaExpression(params IExpression[] children)
    {
        _children = children;
    }
}
public class OrExpression : MetaExpression
{
    public override bool Evaluate(object? arg) => _evaluate(arg, true);
    public OrExpression(params IExpression[] children) : base(children) { }
}
public class AndExpression : MetaExpression
{
    public override bool Evaluate(object? arg) => _evaluate(arg, false);
    public AndExpression(params IExpression[] children) : base(children) { }
}
public static class ExpressionExampleForMyBrain
{    
    public static IExpression MakeExpression()
    {
        static Func<Item?, bool> hasTag(string tag) => (item) => item?.ItemSources.Any(x => x.Tags.Contains(tag)) ?? false;
        return new AndExpression(
            new OrExpression(
                    new GenericExpression<Item>(hasTag("example1"))
                )
            );
    }
}