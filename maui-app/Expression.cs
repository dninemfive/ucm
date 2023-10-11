using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace d9.ucm;
public interface IExpression
{
    public bool Evaluate();
}
public abstract class ExpressionWithChildren : IExpression
{
    public IEnumerable<IExpression> Children => _children;
    private List<IExpression> _children = new();
    public ExpressionWithChildren(params IExpression[] children)
    {
        _children = children.ToList();
    }
    public abstract bool Evaluate();
}
public class OrExpression : ExpressionWithChildren
{
    public override bool Evaluate()
    {
        foreach(IExpression child in Children)
        {
            if (child.Evaluate())
                return true;
        }
        return false;
    }
}
public class AndExpression : ExpressionWithChildren
{
    public override bool Evaluate()
    {
        foreach(IExpression child in Children)
        {
            if (!child.Evaluate())
                return false;
        }
        return true;
    }
}
public class ExpressionOnData<T> : IExpression
{
    private Func<T, bool> _expr;
    public ExpressionOnData(Func<T, bool> expr)
    {
        _expr = expr;
    }
}