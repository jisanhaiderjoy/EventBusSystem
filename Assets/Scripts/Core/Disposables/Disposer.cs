using System;
using System.Collections.Generic;

namespace Core.Disposables
{
    /// <summary>
    /// Collects <see cref="IDisposable"/> instances and disposes them all at once
    /// when this disposer is itself disposed. Intended to be registered as
    /// <c>Lifetime.Scoped</c> in a VContainer <c>LifetimeScope</c> so that the
    /// container owns and triggers cleanup automatically at scope teardown.
    /// </summary>
    public sealed class Disposer : IDisposable
    {
        private readonly List<IDisposable> _disposables = new();
        private bool _disposed;

        /// <summary>
        /// Adds a disposable to be cleaned up when this disposer is disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if this <see cref="Disposer"/> has already been disposed.
        /// </exception>
        public void Add(IDisposable disposable)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Disposer));

            _disposables.Add(disposable);
        }

        /// <summary>
        /// Disposes all registered disposables in reverse registration order (LIFO).
        /// Safe to call multiple times — subsequent calls are no-ops.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            for (var i = _disposables.Count - 1; i >= 0; i--)
                _disposables[i].Dispose();

            _disposables.Clear();
        }
    }
}
