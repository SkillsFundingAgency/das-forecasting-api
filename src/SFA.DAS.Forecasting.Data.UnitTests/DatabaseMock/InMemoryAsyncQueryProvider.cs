﻿using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace SFA.DAS.Forecasting.Data.UnitTests.DatabaseMock;

public class InMemoryAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider innerQueryProvider;

    public InMemoryAsyncQueryProvider(IQueryProvider innerQueryProvider)
    {
        this.innerQueryProvider = innerQueryProvider;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new InMemoryAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new InMemoryAsyncEnumerable<TElement>(expression);
    }

    public object Execute(Expression expression)
    {
        return this.innerQueryProvider.Execute(expression);
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return this.innerQueryProvider.Execute<TResult>(expression);
    }

    public Task<object> ExecuteAsync(Expression expression, CancellationToken cancellationToken)
    {
        return Task.FromResult(this.Execute(expression));
    }


    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
    {
        return this.Execute<TResult>(expression);
    }
}