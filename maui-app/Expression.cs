using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace d9.ucm;
public interface IExpression<T>
{
    public bool Evaluate(T? arg);
}
public class ValueExpression<T> : IExpression<T>
    where T : class
{
    public Func<T?, bool> Expression { get; private set; }
    public bool Evaluate(T? arg) => Expression(arg);

    public ValueExpression(Func<T?, bool> expression)
    {
        Expression = expression;
    }
    public static implicit operator ValueExpression<T>(Func<T?, bool> func) => new(func);
}
public abstract class MetaExpression<T> : IExpression<T>
{
    private readonly IExpression<T>[] _children;
    public IEnumerable<IExpression<T>> Children => _children;
    protected bool _evaluate(T? arg, bool defaultValue)
    {
        foreach(IExpression<T> child in Children)
        {
            if (child.Evaluate(arg) == defaultValue)
                return defaultValue;
        }
        return !defaultValue;
    }
    public abstract bool Evaluate(T? arg);
    public MetaExpression(params IExpression<T>[] children)
    {
        _children = children;
    }
}
public class OrExpression<T> : MetaExpression<T>
{
    public override bool Evaluate(T? arg) => _evaluate(arg, true);
    public OrExpression(params IExpression<T>[] children) : base(children) { }
}
public class AndExpression<T> : MetaExpression<T>
{
    public override bool Evaluate(T? arg) => _evaluate(arg, false);
    public AndExpression(params IExpression<T>[] children) : base(children) { }
}
public static class ExpressionExampleForMyBrain
{    
    public static IExpression<Item> MakeExpression()
    {
        static Func<Item?, bool> hasTag(string tag) => (item) => item?.ItemSources.Any(x => x.Tags.Contains(tag)) ?? false;
        return new AndExpression<Item>(
            new OrExpression<Item>(
                    new ValueExpression<Item>(hasTag("example1"))
                )
            );
    }
}