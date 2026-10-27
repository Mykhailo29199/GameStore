using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace GameStoreUnitTests.GameStore.Services.Tests;

/// <summary>
/// An in-memory query that supports the asynchronous Entity Framework operators (CountAsync, ToListAsync).
/// </summary>
internal sealed class AsyncQueryable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public AsyncQueryable(IEnumerable<T> items)
        : base(items)
    {
    }

    public AsyncQueryable(Expression expression)
        : base(expression)
    {
    }

    IQueryProvider IQueryable.Provider => new AsyncQueryProvider(this);

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new AsyncEnumerator(this.AsEnumerable().GetEnumerator());
    }

    private sealed class AsyncQueryProvider(IQueryProvider inner) : IAsyncQueryProvider
    {
        public IQueryable CreateQuery(Expression expression)
        {
            return new AsyncQueryable<T>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new AsyncQueryable<TElement>(expression);
        }

        public object? Execute(Expression expression)
        {
            return inner.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return inner.Execute<TResult>(expression);
        }

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var resultType = typeof(TResult).GetGenericArguments()[0];

            var result = typeof(IQueryProvider)
                .GetMethod(nameof(IQueryProvider.Execute), 1, [typeof(Expression)])!
                .MakeGenericMethod(resultType)
                .Invoke(inner, [expression]);

            return (TResult)typeof(Task)
                .GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(resultType)
                .Invoke(null, [result])!;
        }
    }

    private sealed class AsyncEnumerator(IEnumerator<T> inner) : IAsyncEnumerator<T>
    {
        public T Current => inner.Current;

        public ValueTask<bool> MoveNextAsync()
        {
            return ValueTask.FromResult(inner.MoveNext());
        }

        public ValueTask DisposeAsync()
        {
            inner.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
