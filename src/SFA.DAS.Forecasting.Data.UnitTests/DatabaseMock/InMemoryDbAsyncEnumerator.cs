using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Data.UnitTests.DatabaseMock
{
    public class InMemoryDbAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> innerEnumerator;
        private bool disposed = false;

        public InMemoryDbAsyncEnumerator(IEnumerator<T> enumerator)
        {
            this.innerEnumerator = enumerator;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            return Task.FromResult(this.innerEnumerator.MoveNext());
        }

        public T Current => this.innerEnumerator.Current;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                    this.innerEnumerator.Dispose();
                }

                this.disposed = true;
            }
        }
        public async ValueTask<bool> MoveNextAsync()
        {
            return await Task.FromResult(this.innerEnumerator.MoveNext());
        }

        public async ValueTask DisposeAsync()
        {
            if (!this.disposed)
            {
                // Dispose managed resources.
                this.innerEnumerator.Dispose();
                this.disposed = true;
            }
        }

    }
}