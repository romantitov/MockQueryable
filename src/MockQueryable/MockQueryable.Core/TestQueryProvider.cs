﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MockQueryable.Core;

public abstract class TestQueryProvider<T, TExpressionVisitor> : IOrderedQueryable<T>, IQueryProvider
    where TExpressionVisitor : ExpressionVisitor, new()
{
    private IEnumerable<T> _enumerable;


    protected TestQueryProvider(Expression expression)
    {
        Expression = expression;
    }

    protected TestQueryProvider(IEnumerable<T> enumerable)
    {
        _enumerable = enumerable;
        Expression = enumerable.AsQueryable().Expression;

    }

    public IQueryable CreateQuery(Expression expression)
    {
        if (expression is MethodCallExpression m)
        {
            var resultType = m.Method.ReturnType; // it should be IQueryable<T>
            var tElement = resultType.GetGenericArguments().First();
            return (IQueryable)CreateInstance(tElement, expression);
        }

        return CreateQuery<T>(expression);
    }

    public IQueryable<TEntity> CreateQuery<TEntity>(Expression expression)
    {
        return (IQueryable<TEntity>)CreateInstance(typeof(TEntity), expression);
    }

    protected abstract object CreateInstance(Type tElement, Expression expression);
    

    public object Execute(Expression expression)
    {
        return CompileExpressionItem<object>(expression);
    }

    public virtual TResult Execute<TResult>(Expression expression)
    {

        return CompileExpressionItem<TResult>(expression);
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        _enumerable ??= CompileExpressionItem<IEnumerable<T>>(Expression);
        return _enumerable.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        _enumerable ??= CompileExpressionItem<IEnumerable<T>>(Expression);
        return _enumerable.GetEnumerator();
    }

    public Type ElementType => typeof(T);

    public Expression Expression { get; }

    public IQueryProvider Provider => this;

    private static TResult CompileExpressionItem<TResult>(Expression expression)
    {
        var visitor = new TExpressionVisitor();
        var body = visitor.Visit(expression);
        var f = Expression.Lambda<Func<TResult>>(
            body ?? throw new InvalidOperationException($"{nameof(body)} is null"),
            (IEnumerable<ParameterExpression>)null);
        return f.Compile()();
    }
}