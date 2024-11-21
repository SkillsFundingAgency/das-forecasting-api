namespace SFA.DAS.Forecasting.Data.UnitTests.DatabaseMock;

public class InMemoryDbAsyncEnumerator<T>(IEnumerator<T> enumerator) : IAsyncEnumerator<T>
{
    private bool _disposed = false;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public Task<bool> MoveNext(CancellationToken cancellationToken)
    {
        return Task.FromResult(enumerator.MoveNext());
    }

    public T Current => enumerator.Current;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources.
                enumerator.Dispose();
            }

            _disposed = true;
        }
    }
    public async ValueTask<bool> MoveNextAsync()
    {
        return await Task.FromResult(enumerator.MoveNext());
    }

    public ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            // Dispose managed resources.
            enumerator.Dispose();
            _disposed = true;
        }

        return default;
    }
}