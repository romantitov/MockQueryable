using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using MockQueryable.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MockQueryable.EntityFrameworkCore;

public class TestAsyncEnumerableEfCore<T, TExpressionVisitor> : TestQueryProvider<T, TExpressionVisitor>,
    IAsyncEnumerable<T>, IAsyncQueryProvider
    where TExpressionVisitor : ExpressionVisitor, new()
{
    private readonly Action<T> _removeCallback;


    public TestAsyncEnumerableEfCore(Expression expression, Action<T> removeCallback) : base(expression)
    {
        _removeCallback = removeCallback;
    }


    public TestAsyncEnumerableEfCore(IEnumerable<T> enumerable, Action<T> removeCallback) : base(enumerable)
    {
        _removeCallback = removeCallback;
    }

    protected override object CreateInstance(Type tElement, Expression expression)
    {
        var queryType = GetType().GetGenericTypeDefinition().MakeGenericType(tElement, typeof(TExpressionVisitor));
        return typeof(T) == tElement 
            ? Activator.CreateInstance(queryType, expression, _removeCallback) 
            : Activator.CreateInstance(queryType, expression, null);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var expectedResultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider)
            .GetMethods()
            .First(method => method.Name == nameof(IQueryProvider.Execute) && method.IsGenericMethod)
            .MakeGenericMethod(expectedResultType)
            .Invoke(this, [expression]);

        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
            .MakeGenericMethod(expectedResultType)
            .Invoke(null, [executionResult]);
    }

    public override TResult Execute<TResult>(Expression expression)
    {
        // Intercept ExecuteDelete and ExecuteUpdate calls
        if (expression is MethodCallExpression
            {
                Method.Name: nameof(EntityFrameworkQueryableExtensions.ExecuteUpdate)
                or nameof(EntityFrameworkQueryableExtensions.ExecuteDelete)
            } methodCall &&
            typeof(TResult) == typeof(int))
        {
            var affectedItems = base.Execute<IEnumerable<T>>(Expression).ToList();

            if (methodCall.Method.Name == nameof(EntityFrameworkQueryableExtensions.ExecuteUpdate))
            {
                ApplyUpdateChangesToDbSet(affectedItems, methodCall);
            }

            if (methodCall.Method.Name == nameof(EntityFrameworkQueryableExtensions.ExecuteDelete))
            {
                foreach (var item in affectedItems)
                {

                    _removeCallback?.Invoke(item);
                }
            }

            // Return the count of affected items
            return (TResult)(object)affectedItems.Count;
        }

        // Fall back to default expression execution
        return base.Execute<TResult>(expression);
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    private static void ApplyUpdateChangesToDbSet(IEnumerable<T> affectedItems,
        MethodCallExpression methodCallExpression)
    {

        if (methodCallExpression.Arguments[1] is not NewArrayExpression arrayExpr)
        {
            return;
        }


        foreach (var element in arrayExpr.Expressions.Cast<NewExpression>())
        {
            var lambdaExpr = ExtractLambda(element.Arguments[0]);
            

            foreach (var item in affectedItems)
            {
                var value = ExtractValue(element.Arguments[1], item);
                SetProperty(item, lambdaExpr, value);
            }
            
            
        }


    }

    private static object ExtractValue(Expression expr, T item)
    {

        if (expr is UnaryExpression { NodeType: ExpressionType.Quote } u)
        {
            expr = u.Operand;
        }

        // 1. Simply constants: "Unit Test", 123, DateTime.Now...
        if (expr is ConstantExpression constExpr)
            return constExpr.Value;


        // 2. Lambdas: x => x.DateOfBirth.AddYears(1) or () => DateTime.UtcNow
        if (expr is LambdaExpression lambda)
        {
            var del = lambda.Compile();

            return lambda.Parameters.Count switch
            {
                0 => del.DynamicInvoke(),                // () => something
                1 => del.DynamicInvoke(item),            // x => something with x (item)
                _ => throw new InvalidOperationException(
                    "Supported only lambdas with 0 or 1 params.")
            };
        }

        

        var lambdaNoParams = Expression.Lambda(expr);
        return lambdaNoParams.Compile().DynamicInvoke();
    }

    private static LambdaExpression ExtractLambda(Expression expr)
    {

        if (expr is UnaryExpression { NodeType: ExpressionType.Quote } unary)
        {
            expr = unary.Operand;
        }

        if (expr is UnaryExpression { NodeType: ExpressionType.Convert } unary2)
        {
            expr = unary2.Operand;
        }

        return expr as LambdaExpression;
    }



    private static void SetProperty(T item, LambdaExpression lambda, object value)
    {
        var body = lambda.Body;


        if (body is UnaryExpression { NodeType: ExpressionType.Convert } unary)
        {
            body = unary.Operand;
        }

        if (body is not MemberExpression memberExpr)
        {
            return;
        }

        if (memberExpr.Member is not PropertyInfo prop)
        {
            return;
        }

        var converted = value;

        if (value != null && !prop.PropertyType.IsInstanceOfType(value))
        {
            converted = Convert.ChangeType(value, prop.PropertyType);
        }

        prop.SetValue(item, converted);
    }


}